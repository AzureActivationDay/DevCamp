using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Owin;
using Overview.Models;
using Database = Microsoft.Azure.Documents.Database;
using Microsoft.Azure.Documents;

namespace Overview.Repositories
{
    public interface IRepository<T> where T:new()
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(string id);
        Task<IEnumerable<T>> GetAll(Expression<Func<T,int,bool>> valueExpression);
        Task<string> Add(T value);
        Task Delete(string id);
        Task<string> Update(string id, T value);
    }

    //public class ValuesRepository : IRepository<Values>
    //{
    //    private readonly List<Values> _allValues =

    //        new Values[]
    //        {
    //            new Values
    //            {
    //                name = "We will do as we say we will",
    //                description = "Value number 1"
    //            },

    //            new Values
    //            {
    //                name = "We will not do as we say we will",
    //                description = "Value number 2"
    //            },

    //            new Values
    //            {
    //                name = "Not sure about 1 or 2 above",
    //                description = "Value number 3"
    //            }

    //        }.ToList();


    //    public Task<IEnumerable<Values>> GetAll()
    //    {
    //        return Task.FromResult<IEnumerable<Values>>(_allValues.ToArray());
    //    }

    //    public Task<Values> GetById(string id)
    //    {
    //        return Task.FromResult(_allValues.FirstOrDefault(x => x.id == id));
    //    }

    //    public Task<IEnumerable<Values>> GetAll(Expression<Func<Values,int,bool>> valueExpression)
    //    {
    //        var allElements= _allValues.FindAll(valueExpression);
    //        if (allElements.Any())
    //        {
    //            return Task.FromResult(allElements.AsEnumerable());
    //        }
    //        return Task.FromResult(Enumerable.Empty<Values>());
    //    }

    //    public void Add(Values value)
    //    {
    //        _allValues.Add(value);
    //    }

    //    public void Delete(string id)
    //    {
    //        _allValues.RemoveAll(x => x.id == id);
    //    }
    //}

    public class ValuesDocumentRepository:IRepository<Values>
    {
        private static readonly string databaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private static readonly string collectionId = ConfigurationManager.AppSettings["CollectionId"];

        private readonly DocumentClient _documentClient;
        public ValuesDocumentRepository(DocumentClient documentClient)
        {
            _documentClient = documentClient;
        }

        public Task<IEnumerable<Values>> GetAll()
        {
            var feedOptions=new FeedOptions {MaxItemCount = 1000};

            var query = _documentClient.CreateDocumentQuery<Values>(
                        GetCollectionSelfLink(), feedOptions)
                        .Where(x=>x.name!=null).AsEnumerable(); //we make sure that value is the only valid type that is returned
                                                                //if collection has various other types of schema.
                                                                //may be a docmentType should be inserted to identify the document type
            var response=query.ToArray();

            if (!response.Any())
            {
                return Task.FromResult(Enumerable.Empty<Values>());
            }
           
            //Just filter out any documents that are not of type values
            //this will happen if you store different type of documents in the same collections.
            //here we are assuming that name is used as qualifier to indicate that the document is of type value if name is present.
            return Task.FromResult(response.AsEnumerable());
        }

        private string GetCollectionSelfLink()
        {
            DocumentCollection dc = _documentClient.CreateDocumentCollectionQuery(GetDbLink())
                .Where(x => x.Id == collectionId)
                .AsEnumerable()
                .FirstOrDefault();
            return dc.SelfLink;
        }

        private string GetDbLink()
        {
            Database db = _documentClient
                .CreateDatabaseQuery()
                .Where(x => x.Id == databaseId)
                .AsEnumerable()
                .FirstOrDefault();

            return db.SelfLink;
        }

        //private async Task<string> GetValuesDocumentLink()
        //{

        //    //Values document exists in a collection called 
        //    //Morals collection

        //    Database db=_documentClient.CreateDatabaseQuery().Where(x => x.Id == databaseId).AsEnumerable().FirstOrDefault();

        //    //var databaseAccount = await _documentClient.ReadDocumentCollectionAsync();

        //    //var dbLink = databaseAccount.SelfLink;
        //    //string documentsLink = String.Empty;
        //    //return documentsLink;

        //    var collection = _documentClient.CreateDocumentCollectionQuery(db.SelfLink).FirstOrDefault(x => x.Id == collectionId);
        //   return coll

        //}

        public Task<Values> GetById(string id)
        {
            var response =  GetByIdInternal(id);
            return Task.FromResult(response);
        }

        private Values GetByIdInternal(string id)
        {
            dynamic doc = this.GetByIdQuery(id);
            Values value = doc;
            return value;
            
            //FeedResponse<Document> response = await query.ExecuteNextAsync<Document>();
            //if (!response.Any())
            //{
            //    throw new ArgumentOutOfRangeException("id");
            //}
            //var resource = response.FirstOrDefault();
            //return resource;
        }

        private dynamic GetByIdQuery(string id)
        {
            //var query = _documentClient.CreateDocumentQuery<Document>(
            //    GetCollectionSelfLink(), null)
            //    .Where(x => x.Id == id)
            //    .AsDocumentQuery();
            //return query;

            dynamic doc = _documentClient.CreateDocumentQuery<Document>(GetCollectionSelfLink())
                .Where(d => d.Id == id).AsEnumerable().FirstOrDefault();

            return doc;

        }

        public async Task<IEnumerable<Values>> GetAll(Expression<Func<Values,int,bool>> valueExpression)
        {
            var query = _documentClient.CreateDocumentQuery<Values>(
                       GetCollectionSelfLink())
                       .Where(valueExpression)
                       .AsDocumentQuery();

            FeedResponse<Values> response = await query.ExecuteNextAsync<Values>();
            return response;
        }

        public async Task<string> Add(Values value)
        {
            value.id = Guid.NewGuid().ToString();
            var options=new RequestOptions();
            //Consistency options
            options.ConsistencyLevel = ConsistencyLevel.Eventual;
            //Access permissions
            //options.AccessCondition=new AccessCondition();
            //options.IndexingDirective=new IndexingDirective();


            try
            {
                ResourceResponse<Document> response=await _documentClient.CreateDocumentAsync(GetCollectionSelfLink(), value, options, true);
                return response.Resource.SelfLink;
            }
            catch (DocumentClientException de)
            {
                //expecting an Forbidden exception, anything else, rethrow
                if ( (de.StatusCode != HttpStatusCode.Forbidden) || (de.StatusCode != HttpStatusCode.Unauthorized))
                {
                    throw new HttpException("Unauthorized",de);   
                };

                throw;
            }
        }

        public async Task Delete(string id)
        {
            var resource = this.GetByIdQuery(id);

            if (resource!=null)
            {
                try
                {
                    await _documentClient.DeleteDocumentAsync(resource.SelfLink, null);
                }
                catch (DocumentClientException de)
                {
                    if ((de.StatusCode != HttpStatusCode.Forbidden) || (de.StatusCode != HttpStatusCode.Unauthorized))
                    {
                        throw new HttpException("Unauthorized", de);
                    };
                    
                    throw;
                }
            }
            else
            {
                throw new ApplicationException("Could not retrieve resource");
            }
            
            
        }

        public async Task<string> Update(string id, Values value)
        {
            var options = new RequestOptions {ConsistencyLevel = ConsistencyLevel.Eventual};
            //Consistency options
            //Access permissions
            //options.AccessCondition=new AccessCondition();
            //options.IndexingDirective=new IndexingDirective();

            Document document = GetByIdQuery(id);;
            
            try
            {
                ResourceResponse<Document> response = await _documentClient.ReplaceDocumentAsync(document.SelfLink, value);
                return response.Resource.SelfLink;
            }
            catch (DocumentClientException de)
            {
                //expecting an Forbidden exception, anything else, rethrow
                if ((de.StatusCode != HttpStatusCode.Forbidden) || (de.StatusCode != HttpStatusCode.Unauthorized))
                {
                    throw new HttpException("Unauthorized", de);
                };

                throw;
            }
        }
    }

    public class DocumentClientFactory:IDisposable
    {
        private static readonly string EndpointUrl = ConfigurationManager.AppSettings["EndPointUrl"];
        private static readonly string AuthorizationKey = ConfigurationManager.AppSettings["AuthorizationKey"];
        
        private readonly DocumentClient _documentClient;
       
        public DocumentClientFactory()
        {
            _documentClient=new DocumentClient(new Uri(EndpointUrl), AuthorizationKey);
        }

        public DocumentClient GetInstance()
        {
            return _documentClient;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            Debug.WriteLine("Disponse on DocumentClient Factory called");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_documentClient != null) _documentClient.Dispose();
            }
        }
    }
}
