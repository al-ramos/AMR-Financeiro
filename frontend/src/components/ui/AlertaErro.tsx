interface Props { mensagem: string; }

export function AlertaErro({ mensagem }: Props) {
  return (
    <div className="alert alert-danger d-flex align-items-center gap-2 py-2 mb-0" style={{ fontSize: 13 }}>
      <i className="bi bi-exclamation-triangle-fill"></i>
      {mensagem}
    </div>
  );
}
