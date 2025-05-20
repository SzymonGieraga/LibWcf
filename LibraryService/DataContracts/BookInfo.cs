using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LibraryService.DataContracts
{
    [DataContract(Name = "Book", Namespace = "")]
    public class Book
    {
        [DataMember]
        public int id;
        [DataMember]
        public string title;
        [DataMember]
        public Author[] authors;
    }
}
