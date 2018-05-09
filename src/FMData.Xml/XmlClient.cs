using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FMData.Xml
{
    public class XmlClient : IFileMakerApiClient
    {
        public Task<IResponse> CreateAsync<T>(T input)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> CreateAsync<T>(string layout, T input)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> CreateAsync<T>(ICreateRequest<T> req)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> DeleteAsync(IDeleteRequest req)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> DeleteAsync<T>(int recId, T delete)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> DeleteAsync(int recId, string layout)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> EditAsync(IEditRequest<Dictionary<string, string>> req)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> EditAsync<T>(IEditRequest<T> req)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> EditAsync<T>(int recordId, T input)
        {
            throw new NotImplementedException();
        }

        public Task<IResponse> EditAsync<T>(string layout, int recordId, T input)
        {
            throw new NotImplementedException();
        }

        public Task<IFindResponse<Dictionary<string, string>>> FindAsync(IFindRequest<Dictionary<string, string>> req)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> FindAsync<T>(IFindRequest<Dictionary<string, string>> req)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> FindAsync<T>(IFindRequest<T> req)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> FindAsync<T>(T request)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> FindAsync<T>(string layout, T request)
        {
            throw new NotImplementedException();
        }
    }
}
