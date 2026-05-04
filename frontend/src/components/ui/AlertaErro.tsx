interface Props {
  mensagem: string;
}

export function AlertaErro({ mensagem }: Props) {
  return (
    <div className="rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
      ⚠ {mensagem}
    </div>
  );
}
