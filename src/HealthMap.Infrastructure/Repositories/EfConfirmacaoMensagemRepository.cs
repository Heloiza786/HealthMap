using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HealthMap.Infrastructure.Repositories;

/// <summary>
/// Repositório de <see cref="ConfirmacaoMensagem"/> baseado em Entity Framework Core (banco SQL).
/// </summary>
public class EfConfirmacaoMensagemRepository : IConfirmacaoMensagemRepository
{
    private readonly AppDbContext _db;

    public EfConfirmacaoMensagemRepository(AppDbContext db) => _db = db;

    public IEnumerable<ConfirmacaoMensagem> GetAll() => _db.ConfirmacoesMensagem.AsNoTracking().ToList();

    public ConfirmacaoMensagem? GetById(string id) =>
        _db.ConfirmacoesMensagem.AsNoTracking().FirstOrDefault(m => m.IdMensagem == id);

    public IEnumerable<ConfirmacaoMensagem> GetByConsulta(string idConsulta) =>
        _db.ConfirmacoesMensagem.AsNoTracking().Where(m => m.IdConsulta == idConsulta).ToList();

    public ConfirmacaoMensagem Add(ConfirmacaoMensagem mensagem)
    {
        _db.ConfirmacoesMensagem.Add(mensagem);
        _db.SaveChanges();
        return mensagem;
    }

    public void Update(ConfirmacaoMensagem mensagem)
    {
        _db.ConfirmacoesMensagem.Update(mensagem);
        _db.SaveChanges();
    }

    public void Delete(string id)
    {
        var mensagem = _db.ConfirmacoesMensagem.Find(id);
        if (mensagem is not null)
        {
            _db.ConfirmacoesMensagem.Remove(mensagem);
            _db.SaveChanges();
        }
    }
}
