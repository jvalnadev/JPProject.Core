using JPProject.EntityFrameworkCore.Interfaces;
using JPProject.EntityFrameworkCore.Repository;
using JPProject.Sso.Domain.Interfaces;
using JPProject.Sso.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JPProject.Sso.EntityFramework.Repository.Repository
{
    public class TemplateRepository : Repository<Template>, ITemplateRepository
    {
        public TemplateRepository(IJpEntityFrameworkStore context) : base(context)
        {
        }

        public Task<bool> Exist(string name)
        {
            return DbSet.AnyAsync(w => w.Name.ToUpper() == name.ToUpper());
        }

        public Task<Template> GetByName(string name)
        {
            return DbSet.FirstOrDefaultAsync(s => s.Name == name);
        }

        public Task<List<Template>> All()
        {
            return DbSet.ToListAsync();
        }
    }
}
