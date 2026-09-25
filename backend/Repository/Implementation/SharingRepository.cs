using backend.Data;
using backend.Models;
using backend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository.Implementation
{
    public class SharingRepository : ISharingRepository
    {
        private readonly CrochetFileShelfDbContext _context;

        public SharingRepository(CrochetFileShelfDbContext context)
        {
            _context = context;
        }

        public Task<WorkInvitation?> GetInvitationByWorkAsync(Guid workId) =>
            _context.WorkInvitations.FirstOrDefaultAsync(i => i.WorkId == workId);

        public Task<WorkInvitation?> GetActiveInvitationByCodeAsync(string code) =>
            _context.WorkInvitations
                .Include(i => i.Work)
                .FirstOrDefaultAsync(i => i.Code == code && i.IsActive);

        public Task<bool> CodeExistsAsync(string code) =>
            _context.WorkInvitations.AnyAsync(i => i.Code == code);

        public async Task AddInvitationAsync(WorkInvitation invitation) =>
            await _context.WorkInvitations.AddAsync(invitation);

        public void RemoveInvitation(WorkInvitation invitation) =>
            _context.WorkInvitations.Remove(invitation);

        public Task<List<WorkMember>> GetMembersAsync(Guid workId) =>
            _context.WorkMembers
                .Include(m => m.User)
                .Where(m => m.WorkId == workId)
                .OrderBy(m => m.JoinedAt)
                .ToListAsync();

        public Task<WorkMember?> GetMemberAsync(Guid workId, Guid userId) =>
            _context.WorkMembers.FirstOrDefaultAsync(m => m.WorkId == workId && m.UserId == userId);

        public async Task AddMemberAsync(WorkMember member) =>
            await _context.WorkMembers.AddAsync(member);

        public void RemoveMember(WorkMember member) =>
            _context.WorkMembers.Remove(member);

        public Task SaveChangesAsync() =>
            _context.SaveChangesAsync();
    }
}
