using FluentValidation.Results;
using NerdStore.Core.DomainObjects;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NerdStore.Vendas.Domain
{
    public partial class Pedido
    {

        public static int MAX_UNIDADES_ITEM => 15;
        public static int MIN_UNIDADES_ITEM => 1;

        protected Pedido()
        {
            _pedidoItems = new List<PedidoItem>();
        }

        public Guid ClienteId { get; private set; }

        public decimal ValorTotal { get; set; }
        public decimal Desconto { get; private set; }

        public PedidoStatus PedidoStatus { get; private set; }

        public bool VoucherUtilizado { get; private set; }

        public Voucher Voucher { get; private set; }

        private readonly List<PedidoItem> _pedidoItems;

        public IReadOnlyCollection<PedidoItem> PedidoItems => _pedidoItems;

        public ValidationResult AplicarVoucher(Voucher voucher)
        {
            var result = voucher.ValidarSeAplicavel();

            if (!result.IsValid) return result;

            Voucher = voucher;
            VoucherUtilizado = true;

            CalcularValorTotalDesconto();

            return result;
        }

        public void CalcularValorTotalDesconto()
        {
            if (!VoucherUtilizado) return;

            decimal desconto = 0;
            var valor = ValorTotal;

            if (Voucher.TipoDescontoVoucher == TipoDescontoVoucher.Valor)
            {
                desconto = Voucher.ValorDesconto.Value;
                valor -= desconto;
            }
            else
            {
                desconto = (ValorTotal * Voucher.PercentualDesconto.Value) / 100;
                valor -= desconto;
            }

            ValorTotal = valor < 0 ? 0 : valor;            
            Desconto = desconto;
        }

        public void AdicionarItem(PedidoItem pedidoItem)
        {
            ValidarQuantidadeItemPermitida(pedidoItem);

            if (pedidoItem.Quantidade < MIN_UNIDADES_ITEM) throw new DomainException($"Mínimo de {MIN_UNIDADES_ITEM} unidades permitidas");

            if (PedidoItemExistente(pedidoItem))
            {
                var itemExistente = _pedidoItems.FirstOrDefault(p => p.ProdutoId == pedidoItem.ProdutoId);

                itemExistente.AdicionarUnidades(pedidoItem.Quantidade);
                pedidoItem = itemExistente;
                _pedidoItems.Remove(itemExistente);
            }

            _pedidoItems.Add(pedidoItem);
            CalcularValorPedido();
        }

        public void AtualizarItem(PedidoItem pedidoItem)
        {
            ValidarPedidoItemInexistente(pedidoItem);
            ValidarQuantidadeItemPermitida(pedidoItem);

            var itemExistente = PedidoItems.FirstOrDefault(p => p.ProdutoId == pedidoItem.ProdutoId);

            _pedidoItems.Remove(itemExistente);
            _pedidoItems.Add(pedidoItem);

            CalcularValorPedido();
        }

        public void RemoverItem(PedidoItem pedidoItems)
        {
            ValidarPedidoItemInexistente(pedidoItems);
            _pedidoItems.Remove(pedidoItems);
            CalcularValorPedido();
        }

        private bool PedidoItemExistente(PedidoItem pedidoItem)
        {
            return _pedidoItems.Any(p => p.ProdutoId == pedidoItem.ProdutoId);
        }

        private void ValidarPedidoItemInexistente(PedidoItem item)
        {
            if (!PedidoItemExistente(item)) throw new DomainException("O item não existe no pedido");
        }

        private void ValidarQuantidadeItemPermitida(PedidoItem item)
        {
            var quantidadeItems = item.Quantidade;
            if (PedidoItemExistente(item))
            {
                var itemExistente = _pedidoItems.FirstOrDefault(p => p.ProdutoId == item.ProdutoId);
                quantidadeItems += itemExistente.Quantidade;
            }

            if (quantidadeItems > MAX_UNIDADES_ITEM) throw new DomainException($"Máximo de {MAX_UNIDADES_ITEM} unidades permitidas");
        }

        private void CalcularValorPedido()
        {
            ValorTotal = PedidoItems.Sum(i => i.CalcularValor());
            CalcularValorTotalDesconto();
        }

        public void TornarRascunho()
        {
            PedidoStatus = PedidoStatus.Rascunho;
        }
    }
}
