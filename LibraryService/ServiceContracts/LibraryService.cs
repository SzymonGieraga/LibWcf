using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using LibraryService.DataContracts;

namespace LibraryService.ServiceContracts
{
    [ServiceContract] // publiczny kontrakt wcf, wszystkie OperationContract sa widoczne dla klientow
    public interface ILibraryService
    {
        [OperationContract]
        int[] findBooksByKeyword(string keyword);

        [OperationContract]
        [FaultContract(typeof(BookNotFound))]
        Book getBookById(int id);
    }
}
