using HealthMap.Domain.Entities;

namespace HealthMap.Domain.Interfaces;

public interface IConfirmacaoMensagemRepository
{
    IEnumerable<ConfirmacaoMensagem> GetAll();
    ConfirmacaoMensagem? GetById(string id);
    IEnumerable<ConfirmacaoMensagem> GetByConsulta(string idConsulta);
    ConfirmacaoMensagem Add(ConfirmacaoMensagem mensagem);
    void Update(ConfirmacaoMensagem mensagem);
    void Delete(string id);
}
