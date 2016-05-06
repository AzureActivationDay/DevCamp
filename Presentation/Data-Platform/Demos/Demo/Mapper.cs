using System.Collections.Generic;
using System.Linq;
using Data_Platform_Demos.DocumentDB;
using Data_Platform_Demos.SqlDatabase;
using Product = Data_Platform_Demos.DocumentDB.Product;

namespace Data_Platform_Demos
{
	/// <summary>
	/// We are using a simple hard coded mapper. Any type of "auto mapper" can be utilized here!
	/// </summary>
	public static class Mapper
	{
		public static Product ToDocumentDatabaseProducts(SqlDatabase.Product sqlProduct)
		{
			return new Product
			{
				id = sqlProduct.ProductID.ToString(),
				Name = sqlProduct.Name,
				Line = sqlProduct.ProductLine,
				Number = sqlProduct.ProductNumber,
				Model = ToModel(sqlProduct.ProductModel),
				Reviews = ToReviews(sqlProduct.ProductReviews),
				Subcategory = ToSubcategory(sqlProduct.ProductSubcategory)
			};
		}

		private static Subcategory ToSubcategory(ProductSubcategory productSubcategory)
		{
			if (productSubcategory == null) return null;

			return new Subcategory
			{
				SubcategoryId = productSubcategory.ProductSubcategoryID,
				Name = productSubcategory.Name,
				Category = ToCategory(productSubcategory.ProductCategory)
			};
		}

		private static Category ToCategory(ProductCategory productCategory)
		{
			if (productCategory == null) return null;

			return new Category
			{
				CategoryId = productCategory.ProductCategoryID,
				Name = productCategory.Name
			};
		}

		private static Review[] ToReviews(IEnumerable<ProductReview> productReviews)
		{
			if (productReviews == null || !productReviews.Any()) return null;

			return productReviews.Select(productReview => new Review
			{
				Comments = productReview.Comments,
				EmailAddress = productReview.EmailAddress,
				ModifiedDate = productReview.ModifiedDate,
				Rating = productReview.Rating,
				ReviewDate = productReview.ReviewDate,
				ReviewerName = productReview.ReviewerName
			}).ToArray();
		}

		private static Model ToModel(ProductModel productModel)
		{
			if (productModel == null) return null;

			return new Model
			{
				CatalogDescription = productModel.CatalogDescription,
				Instructions = productModel.Instructions,
				ModifiedDate = productModel.ModifiedDate,
				Name = productModel.Name,
				Description = ToDescriptions(productModel),
			};
		}

		private static Description[] ToDescriptions(ProductModel productModel)
		{
			if (productModel.ProductModelProductDescriptionCultures == null || !productModel.ProductModelProductDescriptionCultures.Any()) return null;

			return productModel.ProductModelProductDescriptionCultures.Select(productModelProductDescriptionCulture => new Description
			{
				CultureName = productModelProductDescriptionCulture.Culture.Name,
				DescriptionText = productModelProductDescriptionCulture.ProductDescription.Description,
				ModifiedDate = productModelProductDescriptionCulture.ProductDescription.ModifiedDate
			}).ToArray();
		}
	}
}