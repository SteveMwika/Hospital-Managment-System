using Hospital_Managment_System.Data;
using Hospital_Managment_System.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hospital_Managment_System.Services
{
    public class LabTestService : ILabTestService
    {
        private readonly ApplicationDbContext _context;

        public LabTestService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LabTest>> GetAllLabTestsAsync()
        {
            return await _context.LabTests.Include(l => l.Appointment).ToListAsync();
        }

        public async Task<LabTest> GetLabTestByIdAsync(int id)
        {
            return await _context.LabTests.Include(l => l.Appointment).FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task CreateLabTestAsync(LabTest labTest)
        {
            labTest.TestDate = DateTime.UtcNow;  // Set default test date
            _context.LabTests.Add(labTest);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLabTestAsync(LabTest labTest)
        {
            _context.LabTests.Update(labTest);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLabTestAsync(int id)
        {
            var labTest = await GetLabTestByIdAsync(id);
            if (labTest != null)
            {
                _context.LabTests.Remove(labTest);
                await _context.SaveChangesAsync();
            }
        }
    }
}
