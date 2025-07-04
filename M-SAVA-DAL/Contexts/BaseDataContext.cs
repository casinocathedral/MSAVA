using M_SAVA_DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M_SAVA_DAL.Contexts
{
    public class BaseDataContext : DbContext
    {
        public BaseDataContext(DbContextOptions<BaseDataContext> options) : base(options)
        {
            //todo
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //todo
        }
        public DbSet<UserDB> Users { get; set; }
        public DbSet<SavedFileReferenceDB> SavedFiles { get; set; }
        public DbSet<JwtDB> Jwts { get; set; }
        public DbSet<SavedFileDataDB> FileDatas { get; set; }
        public DbSet<InviteCodeDB> InviteCodes { get; set; }
        public DbSet<AccessCodeDB> AccessCodes { get; set; }
        public DbSet<AccessGroupDB> AccessGroups { get; set; }
    }
}
