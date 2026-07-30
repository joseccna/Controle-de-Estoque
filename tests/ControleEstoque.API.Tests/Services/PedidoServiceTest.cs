using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControleEstoque.API.Data;
using ControleEstoque.API.DTOs;
using ControleEstoque.API.Models;
using ControleEstoque.API.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit.Sdk;

namespace ControleEstoque.API.Tests.Services
{
    public class PedidoServiceTest
    {

        private static AppDbContext CreateContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new AppDbContext(options);
        }



        [Fact]
        public async Task CriarPedidoAsync_ComEstoqueSuficiente_DeveCriarPedidoEDeduzirEstoque()
        {
            // Arrage: Pupuloar o banco em memória com cliente, forma de pagamento e produtos (estoque = 10)
            using var context = CreateContext(Guid.NewGuid().ToString());

            var service = new PedidoService(context);
            var cliente = new Cliente { Id = 1, Email = "cliente@teste.com",Perfil = PerfilUsuario.Cliente };
            var produto = new Produto { Id = 1, Nome = "Produto Teste", FornecedorId = 1 , QuantidadeEstoque = 10 };
            var formaPagamento = new FormaPagamento { Id = 1, Nome = "Cartão de Crédito" , Ativo = true };

            context.Clientes.Add(cliente);
            context.Produtos.Add(produto);
            context.FormasPagamento.Add(formaPagamento);
            await context.SaveChangesAsync();

            var itens = new List<ItemPedido>
            {
                new ItemPedido { ProdutoId = produto.Id, Quantidade = 3 }
            };

            // Act: Cria pedido compondo 3 unidades do produto



            var pedido = await service.CriarPedidoAsync(cliente.Id, formaPagamento.Id, itens);


            // Assert: Valida que o pedido foi criado e que o estoque do produto foi reduzido de 10 para 7 unidades

            Assert.NotNull(pedido);
            Assert.Equal("Aberto", pedido.Status);

            var produtoNoBanco = await context.Produtos.FindAsync(produto.Id);
            Assert.NotNull(produtoNoBanco);
            Assert.Equal(7, produtoNoBanco.QuantidadeEstoque);
        }




    }


}
