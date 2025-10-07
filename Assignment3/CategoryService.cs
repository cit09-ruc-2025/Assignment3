using Assignment3.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assignment3
{
    public class CategoryService : ICategoryService
    {
        // creating an in-memory database
        private readonly List<Category> _categories;

        public CategoryService()
        {
            // default data initialized
            _categories = new List<Category>
            {
                new Category { cid = 1, name = "Beverages" },
                new Category { cid = 2, name = "Condiments" },
                new Category { cid = 3, name = "Confections" }
            };
        }

        // show all categories
        public List<Category> GetCategories()
        {
            return _categories;
        }

        // show a specific category by ID 
        public Category? GetCategory(int cid)
        {
            return _categories.FirstOrDefault(c => c.cid == cid);
        }

        // create a new category with auto-geterating IDs 
        private int _nextId = 4;

        public Category CreateCategory(string name)
        {
            var cat = new Category { cid = _nextId++, name = name };
            _categories.Add(cat);
            return cat;
        }


        // update an existing category
        public bool UpdateCategory(int id, string newName)
        {
            var category = _categories.FirstOrDefault(c => c.cid == id);
            if (category == null)
                return false;

            category.name = newName;
            return true;
        }

        // delete an existing category 
        public bool DeleteCategory(int id)
        {
            var category = _categories.FirstOrDefault(c => c.cid == id);
            if (category == null)
                return false;

            _categories.Remove(category);
            return true;
        }
    }
}
