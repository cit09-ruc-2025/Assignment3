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
        private List<Category> _categoryList = new List<Category> {
            new Category(1, "Beverages"),
            new Category(2, "Condiments"),
            new Category(3, "Confections")
        };

        public bool CreateCategory(int id, string name)
        {

            _categoryList.Add(new Category(id, name));
            return true;
        }

        public bool DeleteCategory(int id)
        {
            throw new NotImplementedException();
        }

        public List<Category> GetCategories()
        {
            return _categoryList;
        }

        public Category GetCategory(int cid)
        {
            Category category = _categoryList.Find((c) => c.Id == cid);
            return category;
        }

        public bool UpdateCategory(int id, string newName)
        {
            Category category = _categoryList.Find((c) => c.Id == id);

            if (category == null)
            {
                return false;
            }

            category.Name = newName;

            return true;

        }
    }
}
