# Ander Flow Controle 0.1.0

Edição em português brasileiro por **Ander Flow**, baseada no [UniPad](https://github.com/UniPad-app/UniPad/tree/f6756f3416ab6d8250b066bd4e854fce1139c18d), distribuído sob a licença MIT. Os arquivos LICENSE e THIRD-PARTY-NOTICES.md preservam os créditos e as licenças originais. Interface de referência para o fluxo de pareamento: PadlinQ.

## Instalar no Windows

1. Extraia o ZIP. Abra a pasta **Windows** e execute **AnderFlowControle.exe**. É portátil e já inclui o runtime .NET. O Windows precisa ser 10 ou 11, de 64 bits (Intel ou AMD).
2. Caso o programa indique que falta o **ViGEmBus**, clique em **Instalar** no aviso do aplicativo ou baixe o [instalador oficial do ViGEmBus](https://github.com/nefarius/ViGEmBus/releases/tag/v1.22.0). Confirme a permissão de administrador para instalar o driver e reabra o Ander Flow Controle. Se o instalador pedir, reinicie o Windows.
3. Se o Firewall do Windows mostrar um aviso, permita o Ander Flow Controle em **Redes privadas**. Deixe o jogo fechado durante o primeiro pareamento.

## Conectar dois celulares

1. Coloque o Android, o iPhone e o PC na mesma rede local. O PC pode estar ligado ao roteador por cabo.
2. Abra **Celulares** no programa do PC. Escaneie o QR Code com cada celular e abra o link na câmera. No Android, use o Chrome. No iPhone, use o Safari. Dê nomes diferentes para identificar cada aparelho.
3. Em **Jogador 1**, escolha o primeiro celular no campo **Dispositivo de entrada**. Em **Jogador 2**, escolha o outro. Confirme **Conectar controle**, selecione **Xbox 360** para os dois e clique em **Aplicar**. O layout padrão é mapeado ao selecionar o celular.
4. Pressione **Win + R**, digite `joy.cpl` e abra as propriedades de cada controle para conferir se cada celular aciona o seu. Depois, abra o jogo, entre no modo cooperativo local e confirme a entrada do segundo jogador.

Cada celular controla um jogador. A divisão da imagem no monitor depende de o jogo oferecer **tela dividida na versão de PC**.

## Se o QR Code não abrir

- Confira se ambos os celulares estão na rede principal do roteador. Redes de convidados podem bloquear a comunicação com o PC.
- Em **Celulares**, selecione o endereço IP da rede local, se houver mais de um endereço disponível. Gere um novo QR Code e escaneie novamente.
- Se o jogo já estava aberto durante a conexão dos controles, feche e abra o jogo.
- No iPhone, o controle pode ser adicionado à tela de início em **Compartilhar → Adicionar à Tela de Início**, se o Safari oferecer a opção.

## Recursos desta edição

- Pareamento local por QR Code com painel de celulares conectados.
- Controles virtuais Xbox 360 e DualShock 4, com atribuição individual por jogador.
- Controles por toque em modo paisagem, com analógicos, gatilhos e ajuste de tamanho, posição e opacidade dos botões.
- Interface em português no Windows e nos celulares. Nomes de componentes e mensagens nativas do sistema podem aparecer no idioma original.
- A adaptação cobre o controle local. Streaming de vídeo, recorte de vídeo por jogador, pareamento via internet e os layouts específicos do PadlinQ ficam fora desta edição.

## Código-fonte

A pasta **Codigo-fonte** contém os projetos originais adaptados, a licença MIT e os avisos de terceiros. No Windows com o SDK .NET 9 ou 10, execute `build\publish.cmd` para gerar o pacote portátil. O primeiro build usa internet para baixar as dependências.
