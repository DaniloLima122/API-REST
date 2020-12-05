using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Linq;
using API_REST.Data;
using API_REST.Models;
using Microsoft.AspNetCore.Mvc;
using API_REST.HATEOAS;


namespace API_REST.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly ApplicationDbContext database;
        private HATEOAS.HATEOAS HATEOAS;
        public ProdutosController(ApplicationDbContext database)
        {
            this.database = database;
            HATEOAS = new HATEOAS.HATEOAS("localhost:5001/api/v1/Produtos");
            HATEOAS.AddAction("GET_INFO", "GET");
            HATEOAS.AddAction("DELETE_PRODUCT", "DELETE");
            HATEOAS.AddAction("EDIT_PRODUCT", "PATCH");
        }

        [HttpGet]
        public IActionResult Get()
        {
            var produtos = database.Produtos.ToList();

            List<ProdutoContainer> produtosHATEOAS = new List<ProdutoContainer>();

            foreach (var prod in produtos)
            {
                ProdutoContainer produtoHATEOAS = new ProdutoContainer();
                produtoHATEOAS.produto = prod;
                produtoHATEOAS.links = HATEOAS.GetActions(prod.Id.ToString());
                produtosHATEOAS.Add(produtoHATEOAS);
            }

            return Ok(produtosHATEOAS);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                Produto produto = database.Produtos.First(prod => prod.Id == id);

                ProdutoContainer produtoHATEOAS = new ProdutoContainer();

                produtoHATEOAS.produto = produto;

                produtoHATEOAS.links = HATEOAS.GetActions(produto.Id.ToString());

                return Ok(produtoHATEOAS);
            }
            catch (Exception error)
            {
                Response.StatusCode = 404;
                return new ObjectResult(new { erro = "deu erro", error = error.Message });
            }

        }


        [HttpPost]
        public IActionResult Post([FromBody] ProdutoTemp pTemp)
        {
            var nomeInvalido = pTemp.Nome.Trim().ToString().Length <= 1;
            var precoInvalido = pTemp.Preco <= 0;

            if (nomeInvalido)
            {

                Response.StatusCode = 400;
                return new ObjectResult(new { msg = "O nome do produto precisa ter mais de um caracter" });
            }

            if (precoInvalido)
            {

                Response.StatusCode = 400;
                return new ObjectResult(new { msg = "O preço do produto não pode ser menor ou igual a zero" });
            }

            Produto p = new Produto();

            p.Nome = pTemp.Nome;
            p.Preco = pTemp.Preco;

            database.Produtos.Add(p);

            database.SaveChanges();

            Response.StatusCode = 201;

            return new ObjectResult("");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {

                var produto = database.Produtos.First(prod => prod.Id == id);

                database.Produtos.Remove(produto);

                database.SaveChanges();

                return Ok();

            }
            catch
            {

                Response.StatusCode = 404;
                return new ObjectResult("");

            }
        }


        [HttpPatch]
        public IActionResult Patch([FromBody] Produto prod)
        {

            if (prod.Id > 0)
            {

                try
                {

                    var produto = database.Produtos.First(p => p.Id == prod.Id);

                    if (produto != null)
                    {
                        produto.Nome = prod.Nome != null ? prod.Nome : produto.Nome;
                        produto.Preco = prod.Preco != 0 ? prod.Preco : produto.Preco;

                        database.SaveChanges();

                        return Ok();
                    }
                    else
                    {

                        Response.StatusCode = 400;
                        return new ObjectResult(new { msg = "Produto não encontrado" });

                    }

                }
                catch
                {

                    Response.StatusCode = 400;
                    return new ObjectResult(new { msg = "Produto não encontrado" });

                }

            }
            else
            {

                Response.StatusCode = 400;
                return new ObjectResult(new { msg = "Id do produto é inválido" });
            }
        }


        public class ProdutoTemp
        {
            public string Nome { get; set; }
            public float Preco { get; set; }

        }

        public class ProdutoContainer
        {
            public Produto produto { get; set; }
            public Link[] links { get; set; }
        }
    }
}