using System.Collections.Generic;

namespace FMData.Xml.Requests
{
    public class FindRequest<T> : IFindRequest<T>
    {
        public string Layout { get; set; }
        public IEnumerable<T> Query { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public IEnumerable<ISort> Sort { get; set; }
        public string ResponseLayout { get; set; }
        public string Script { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string ScriptParameter { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string PreRequestScript { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string PreRequestScriptParameter { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string PreSortScript { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public string PreSortScriptParameter { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public string SerializeRequest()
        {
            throw new System.NotImplementedException();
        }
    }
}