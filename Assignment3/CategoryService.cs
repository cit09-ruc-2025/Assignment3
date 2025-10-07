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
                new Category(1, "Beverages"),
                new Category(2, "Condiments"),
                new Category(3, "Confections")
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
            return _categories.FirstOrDefault(c => c.Id == cid);
        }

        // create a new category with auto-geterating IDs 
        private int _nextId = 4;

        public bool CreateCategory(int cid, string name)
        {
            if (_categories.Any((c) => c.Id == cid))
            {
                return false;
            }
            var cat = new Category(cid < 0 ? _nextId++ : cid, name);
            _categories.Add(cat);
            return true;
        }


        // update an existing category
        public bool UpdateCategory(int id, string newName)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
                return false;

            category.Name = newName;
            return true;
        }

        // delete an existing category 
        public bool DeleteCategory(int id)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
                return false;

            _categories.Remove(category);
            return true;
        }
    }
}
