using Assignment3.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment3
{
    public class CategoryService : ICategoryService
    {
        private readonly List<Category> _repository = [new Category() { Id = 1, Name = "Beverages" }, new Category() { Id = 2, Name = "Condiments" }, new Category() { Id = 3, Name = "Confections" }];

        public bool CreateCategory(int id, string name)
        {
            if (_repository.Any(c => c.Id == id)) return false;

            _repository.Add(new Category() { Id = id, Name = name });
            return true;
        }

        public bool DeleteCategory(int id)
        {
            var categoryToBeDeleted = _repository.FirstOrDefault(c => c.Id == id);
            if (categoryToBeDeleted == null) return false;  

            _repository.Remove(categoryToBeDeleted);
            return true;
        }

        public List<Category> GetCategories()
        {
            return _repository; 
        }

        public Category GetCategory(int cid)
        {
            return _repository.FirstOrDefault(c => c.Id == cid);
        }

        public bool UpdateCategory(int id, string newName)
        {
            var categoryToBeUpdated = _repository.FirstOrDefault(c => c.Id == id);
            if (categoryToBeUpdated == null) return false;

            categoryToBeUpdated.Name = newName;
            return true;
        }
    }
}
