using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyShop.Core.Contracts;
using MyShop.Core.Models;
using MyShop.Core.ViewModels;
using MyShop.Services;
using MyShop.WebUI.Controllers;
using MyShop.WebUI.Tests.Mocks;
using System;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MyShop.WebUI.Tests.Controllers
{
    [TestClass]
    public class BasketControllerTest
    {
        [TestMethod]
        public void CanAddBasketItem()
        {
            IRepository<Basket> baskets = new MockContext<Basket>();
            IRepository<Product> products = new MockContext<Product>();
            IRepository<Order> orders = new MockContext<Order>();
            IRepository<Customer> customers = new MockContext<Customer>();

            var httpContext = new MockHttpContext();

            IBasketService basketService = new BasketService(products, baskets);
            IOrderService orderService= new OrderService(orders);

            var controller = new BasketController(basketService, orderService, customers);
            controller.ControllerContext = new ControllerContext(httpContext, new RouteData(), controller);
            //basketService.AddToBasket(httpContext, "1");
            controller.AddToBasket("1");


            Basket basket = baskets.Collection().FirstOrDefault();

            Assert.IsNotNull(basket);
            Assert.AreEqual(1, basket.BasketItems.Count);
            Assert.AreEqual("1", basket.BasketItems.ToList().FirstOrDefault().ProductId);
        }

        [TestMethod]
        public void CanGetSummaryViewModel()
        {
            IRepository<Basket> baskets = new MockContext<Basket>();
            IRepository<Product> products = new MockContext<Product>();
            IRepository<Order> orders = new MockContext<Order>();
            IRepository<Customer> customers = new MockContext<Customer>();


            products.Insert(new Product(){ Id = "1", Price = 10});
            products.Insert(new Product(){ Id = "2", Price = 20});

            Basket basket = new Basket();
            basket.BasketItems.Add(new BasketItem() { ProductId = "1", Quantity = 2 });
            basket.BasketItems.Add(new BasketItem() { ProductId = "2", Quantity = 1 });
            baskets.Insert(basket);

            IBasketService basketService = new BasketService(products, baskets);
            IOrderService orderService = new OrderService(orders);




            var controller = new BasketController(basketService , orderService ,customers);
            var httpContext = new MockHttpContext();
            httpContext.Request.Cookies.Add( new HttpCookie("eCommerceBasket") { Value = basket.Id });
            controller.ControllerContext = new ControllerContext(httpContext, new RouteData(), controller);


            var result = controller.BasketSummary() as PartialViewResult;
            var basketSammary = (BasketSummeryViewModel)result.ViewData.Model;


            Assert.AreEqual(3, basketSammary.BasketCount);
            Assert.AreEqual(40, basketSammary.BasketTotal);

        }
        [TestMethod]
        public void CanCheckoutAndCreateOrder()
        {
            IRepository<Product> products = new MockContext<Product>();
            products.Insert(new Product(){Id = "1", Price =50});
            products.Insert(new Product(){Id = "2", Price =100});


            IRepository<Basket> baskets = new MockContext<Basket>();
            Basket basket = new Basket();
            basket.BasketItems.Add(new BasketItem() { ProductId = "1", Quantity = 2, BasketId=basket.Id });
            basket.BasketItems.Add(new BasketItem() { ProductId = "2", Quantity = 1, BasketId = basket.Id });
            baskets.Insert(basket);


            IBasketService basketService = new BasketService(products, baskets);
            IRepository<Order> orders = new MockContext<Order>();
            IOrderService orderService = new OrderService(orders);
            IRepository<Customer> customers = new MockContext<Customer>();


            customers.Insert(new Customer() { Id = "1", Email = "test@test.test", ZipCode = "50" });
            IPrincipal FakeUser = new GenericPrincipal(new GenericIdentity("test@test.test", "forms"), null);

            var controller = new BasketController(basketService, orderService, customers);
            var httpContext = new MockHttpContext();
            httpContext.User = FakeUser;
            httpContext.Request.Cookies.Add(new HttpCookie("eCommerceBasket") { Value = basket.Id });
            controller.ControllerContext = new ControllerContext(httpContext, new RouteData(), controller);

            Order order = new Order();
            controller.Checkout(order);

            Assert.AreEqual(2, order.OrderItems.Count);
            Assert.AreEqual(0, basket.BasketItems.Count);

            Order orderInRep = orders.Find(order.Id);
            Assert.AreEqual(2, orderInRep.OrderItems.Count);
        }
    }
}
