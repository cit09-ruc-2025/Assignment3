using System.Collections.Generic;

namespace Assignment3.Interfaces
{
    public interface ICategoryService
    {
        List<Category> GetCategories();           // Get all categories
        Category? GetCategory(int cid);           // Get category by ID
        Category CreateCategory(string name);     // Create category with auto-generated ID
        bool UpdateCategory(int cid, string newName);  // Update category name by ID
        bool DeleteCategory(int cid);             // Delete category by ID
    }
}
