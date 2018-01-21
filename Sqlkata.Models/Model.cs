using System;
using System.Collections.Generic;
using System.Data;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Sqlkata.Models
{
    public class Model<T> where T : Model<T>
    {
        private XQuery query;
        protected bool touched;
        protected string tableName;
        protected string idColumn = "id";

        protected Dictionary<string, object> currentAttributes = new Dictionary<string, object>();
        protected Dictionary<string, object> newAttributes = new Dictionary<string, object>();

        public Model(IDbConnection connection, Compiler compiler)
        {
            this.query = new XQuery(connection, compiler).From(tableName) as XQuery;
        }

        public T Find(object id)
        {
            return query.Where(idColumn, id).FirstOrDefault<T>();
        }

        public T Save()
        {

            if (touched)
            {
                query.Where(idColumn, this.currentAttributes[idColumn]).Update(newAttributes);
            }
            else
            {
                this.query.Insert(newAttributes);
            }

            return this as T;

        }

        public int Delete()
        {
            return this.query.Where(idColumn, this.currentAttributes[idColumn]).Delete();
        }



    }
}
