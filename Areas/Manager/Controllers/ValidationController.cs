﻿using Microsoft.AspNetCore.Mvc;
using InventorySystem.Models;

namespace InventorySystem.Areas.Manager.Controllers
{
    
    [Area("Manager")]
    public class ValidationController : Controller
    {
        private Repository<Category> categoryData { get; set; }
        private Repository<Warehouse> warehouseData { get; set; }

        public ValidationController(InventorySystemContext ctx)
        { 
            categoryData = new Repository<Category>(ctx);
            warehouseData = new Repository<Warehouse>(ctx);
        }

        public JsonResult CheckWarehouse(string warehouseId)
        {
            var validate = new Validate(TempData);
            validate.CheckWarehouse(warehouseId, warehouseData);
            if (validate.IsValid) {
                validate.MarkWarehouseChecked();
                return Json(true);
            }
            else {
                return Json(validate.ErrorMessage);
            }
        }

        public JsonResult CheckCategory(string name, string operation)
        {
            var validate = new Validate(TempData);
            validate.CheckCategory(name, operation, categoryData);
            if (validate.IsValid) {
                validate.MarkCategoryChecked();
                return Json(true);
            }
            else {
                return Json(validate.ErrorMessage);
            }
        }
    }
}