using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyShop.WebUI.Controllers;
using MyShop.Core.Contracts;
using MyShop.Core.Models;
using System.Web.Mvc;
using MyShop.Core.ViewModels;
using System.Linq;

namespace MyShop.WebUI.Tests.Controllers
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void IndexPageDoesReturnProducts()
        {
            IRepository<Product> productContext = new Mocks.MockContext<Product>();
            IRepository<ProductCategory> productCatgeoryContext = new Mocks.MockContext<ProductCategory>();

            productContext.Insert(new Product());

            HomeController controller = new HomeController(productContext, productCatgeoryContext);

            // "ViewResult" represents the ActionResult that the method in the
            // controller returns
            var result = controller.Index() as ViewResult;
            var viewModel = (ProductListViewModel)result.ViewData.Model;

            // tests if theres a total product count of 1
            Assert.AreEqual(1, viewModel.Products.Count());

        }
    }
}
