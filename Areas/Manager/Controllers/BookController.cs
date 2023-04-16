﻿using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InventorySystem.Models;

namespace InventorySystem.Areas.Admin.Controllers
{
    [Authorize(Roles = "Manager")]
    [Area("Manager")]
    public class BookController : Controller
    {
        private InventorySystemUnitOfWork data { get; set; }
        public BookController(InventorySystemContext ctx) => data = new InventorySystemUnitOfWork(ctx);

        public ViewResult Index() {
            var search = new SearchData(TempData);
            search.Clear();

            return View();
        }

        [HttpPost]
        public RedirectToActionResult Search(SearchViewModel vm)
        {
            if (ModelState.IsValid) {
                var search = new SearchData(TempData) {
                    SearchTerm = vm.SearchTerm,
                    Type = vm.Type
                };
                return RedirectToAction("Search");
            }  
            else {
                return RedirectToAction("Index");
            }   
        }

        [HttpGet]
        public ViewResult Search() 
        {
            var search = new SearchData(TempData);

            if (search.HasSearchTerm) {
                var vm = new SearchViewModel {
                    SearchTerm = search.SearchTerm
                };

                var options = new QueryOptions<Product> {
                    Include = "Warehouse, BookCategories.Category"
                };
                if (search.IsBook) { 
                    options.Where = b => b.Title.Contains(vm.SearchTerm);
                    vm.Header = $"Search results for product title '{vm.SearchTerm}'";
                }
                if (search.IsAuthor) {
                    int index = vm.SearchTerm.LastIndexOf(' ');
                    if (index == -1) {
                        options.Where = b => b.BookCategories.Any(
                            ba => ba.Category.FirstName.Contains(vm.SearchTerm) || 
                            ba.Category.LastName.Contains(vm.SearchTerm));
                    }
                    else {
                        string first = vm.SearchTerm.Substring(0, index);
                        string last = vm.SearchTerm.Substring(index + 1); 
                        options.Where = b => b.BookCategories.Any(
                            ba => ba.Category.FirstName.Contains(first) && 
                            ba.Category.LastName.Contains(last));
                    }
                    vm.Header = $"Search results for author '{vm.SearchTerm}'";
                }
                if (search.IsGenre) {                  
                    options.Where = b => b.GenreId.Contains(vm.SearchTerm);
                    vm.Header = $"Search results for warehouse ID '{vm.SearchTerm}'";
                }
                vm.Products = data.Products.List(options);
                return View("SearchResults", vm);
            }
            else {
                return View("Index");
            }     
        }

        [HttpGet]
        public ViewResult Add(int id) => GetBook(id, "Add");

        [HttpPost]
        public IActionResult Add(BookViewModel vm)
        {
            if (ModelState.IsValid) {
                data.LoadNewBookCategories(vm.Product, vm.SelectedCategories);
                data.Products.Insert(vm.Product);
                data.Save();

                TempData["message"] = $"{vm.Product.Title} added to Products.";
                return RedirectToAction("Index");  
            }
            else {
                Load(vm, "Add");
                return View("Product", vm);
            }
        }

        [HttpGet]
        public ViewResult Edit(int id) => GetBook(id, "Edit");
        
        [HttpPost]
        public IActionResult Edit(BookViewModel vm)
        {
            if (ModelState.IsValid) {
                data.DeleteCurrentBookCategories(vm.Product);
                data.LoadNewBookCategories(vm.Product, vm.SelectedCategories);
                data.Products.Update(vm.Product);
                data.Save(); 
                
                TempData["message"] = $"{vm.Product.Title} updated.";
                return RedirectToAction("Search");  
            }
            else {
                Load(vm, "Edit");
                return View("Product", vm);
            }
        }

        [HttpGet]
        public ViewResult Delete(int id) => GetBook(id, "Delete");

        [HttpPost]
        public IActionResult Delete(BookViewModel vm)
        {
            data.Products.Delete(vm.Product); 
            data.Save();
            TempData["message"] = $"{vm.Product.Title} removed from Products.";
            return RedirectToAction("Search");  
        }

        private ViewResult GetBook(int id, string operation)
        {
            var product = new BookViewModel();
            Load(product, operation, id);
            return View("Product", product);
        }
        private void Load(BookViewModel vm, string op, int? id = null)
        {
            if (Operation.IsAdd(op)) { 
                vm.Product = new Product();
            }
            else {
                vm.Product = data.Products.Get(new QueryOptions<Product>
                {
                    Include = "BookCategories.Category, Warehouse",
                    Where = b => b.BookId == (id ?? vm.Product.BookId)
                });
            }

            vm.SelectedCategories = vm.Product.BookCategories?.Select(
                ba => ba.Category.AuthorId).ToArray();
            vm.Categories = data.Categories.List(new QueryOptions<Category> {
                OrderBy = a => a.FirstName });
            vm.Warehouses = data.Warehouses.List(new QueryOptions<Warehouse> {
                    OrderBy = g => g.Name });
        }

    }
}