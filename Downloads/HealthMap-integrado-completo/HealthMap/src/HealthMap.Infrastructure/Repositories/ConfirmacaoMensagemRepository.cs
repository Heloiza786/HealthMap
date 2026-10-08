using HealthMap.Domain.Entities;
using HealthMap.Domain.Interfaces;
using HealthMap.Infrastructure.Persistence;

namespace HealthMap.Infrastructure.Repositories;

public class ConfirmacaoMensagemRepository : IConfirmacaoMensagemRepository
{
    private readonly JsonDatabase _db;

    public ConfirmacaoMensagemRepository(JsonDatabase db) => _db = db;

    public IEnumerable<ConfirmacaoMensagem> GetAll() => _db.LoadDocument().ConfirmacoesMensagem;

    public ConfirmacaoMensagem? GetById(string id) => _db.LoadDocument().ConfirmacoesMensagem.FirstOrDefault(m => m.IdMensagem == id);

    public IEnumerable<ConfirmacaoMensagem> GetByConsulta(string idConsulta) => _db.LoadDocument().ConfirmacoesMensagem.Where(m => m.IdConsulta == idConsulta);

    public ConfirmacaoMensagem Add(ConfirmacaoMensagem mensagem)
    {
        var doc = _db.LoadDocument();
        doc.ConfirmacoesMensagem.Add(mensagem);
        _db.SaveDocument(doc);
        return mensagem;
    }

    public void Update(ConfirmacaoMensagem mensagem)
    {
        var doc = _db.LoadDocument();
        var idx = doc.ConfirmacoesMensagem.FindIndex(m => m.IdMensagem == mensagem.IdMensagem);
        if (idx >= 0) doc.ConfirmacoesMensagem[idx] = mensagem;
        _db.SaveDocument(doc);
    }

    public void Delete(string id)
    {
        var doc = _db.LoadDocument();
        doc.ConfirmacoesMensagem.RemoveAll(m => m.IdMensagem == id);
        _db.SaveDocument(doc);
    }
}
