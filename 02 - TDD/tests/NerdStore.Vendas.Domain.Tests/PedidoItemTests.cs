using NerdStore.Core.DomainObjects;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NerdStore.Vendas.Domain.Tests
{
    public class PedidoItemTests
    {
        [Fact(DisplayName = "Adicionar Item Pedido com unidades Abaixo do permitido")]
        [Trait("Categoria", "Vendas -  Pedido Item")]
        public void AdicionarItemPedido_UnidadesItemAbaixoDoPermitido_DeveRetornarException()
        {
            // Arrange, Act & Assert
            Assert.Throws<DomainException>(() => new PedidoItem(Guid.NewGuid(), "Produto Teste", Pedido.MIN_UNIDADES_ITEM - 1, 100));
        }
    }
}
