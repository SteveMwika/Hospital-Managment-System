using Hospital_Managment_System.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hospital_Managment_System.Services
{
    public interface ILabTestService
    {
        Task<IEnumerable<LabTest>> GetAllLabTestsAsync();
        Task<LabTest> GetLabTestByIdAsync(int id);
        Task CreateLabTestAsync(LabTest labTest);
        Task UpdateLabTestAsync(LabTest labTest);
        Task DeleteLabTestAsync(int id);
    }
}
