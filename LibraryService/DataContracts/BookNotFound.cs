using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService.DataContracts
{
    [DataContract(Name = "BookNotFound", Namespace = "")]
    public class BookNotFound
    {
        public int bookId;
        public BookNotFound(int bookId)
        {
            this.bookId = bookId;
        }
    }
}
