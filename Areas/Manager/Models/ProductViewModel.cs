﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InventorySystem.Models
{
    public class ProductViewModel : IValidatableObject
    {
        public Product Product { get; set; }
        public IEnumerable<Warehouse> Warehouses { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public int[] SelectedCategories { get; set; }

        [Required(ErrorMessage = "Please select category.")]
        public int CategoryId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext ctx) {
            if (CategoryId == 0)
            {
                yield return new ValidationResult(
                  "Please select category.",
                  new[] { nameof(CategoryId) });
            }
        }

    }
}
