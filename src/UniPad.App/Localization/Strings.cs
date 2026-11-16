using System.ComponentModel;
using Avalonia.Media;

namespace UniPad.App.Localization;

/// <summary>
/// Minimal in-code localisation table.
/// <para>
/// Satellite resource assemblies are avoided on purpose: they complicate single-file publishing.
/// Two languages in a dictionary is entirely adequate here, and switching language raises
/// <see cref="INotifyPropertyChanged"/> for the indexer so bound text updates live.
/// </para>
/// </summary>
public sealed class Strings : INotifyPropertyChanged
{
    /// <summary>Shared instance bound from XAML.</summary>
    public static Strings Instance { get; } = new();

    private string _language = "pt-BR";

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Raised after the active language changes.</summary>
    public event Action? LanguageChanged;

    /// <summary>Active language code, <c>en</c> or <c>fa</c>.</summary>
    public string Language
    {
        get => _language;
        set
        {
            var normalised = value is "fa" ? "fa" : "pt-BR";
            if (_language == normalised)
            {
                return;
            }

            _language = normalised;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRightToLeft)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextFlowDirection)));

            // Tells the bound captions to re-read themselves. Raised before LanguageChanged so that
            // the view models rebuild their own strings on top of an already-updated interface.
            Text.RaiseAllChanged();

            LanguageChanged?.Invoke();
        }
    }

    /// <summary>True when the active language is written right to left.</summary>
    public bool IsRightToLeft => _language == "fa";

    /// <summary>
    /// Base direction for text runs, bound from XAML on individual text leaves.
    /// <para>
    /// Kept separate from the layout direction on purpose: a panel whose columns must not swap is
    /// pinned to left-to-right, but the captions inside it still have to be laid out with the
    /// language's own base direction. Without that, a left-to-right base direction resolves the
    /// zero-width non-joiner inside Persian words as a neutral boundary, splits the word into
    /// separate runs and orders them the wrong way round.
    /// </para>
    /// </summary>
    public FlowDirection TextFlowDirection =>
        IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

    /// <summary>
    /// Wraps a value that is not translated - a number, a unit, a device or profile name, an
    /// exception message - in a first-strong isolate.
    /// <para>
    /// Without it the bidirectional algorithm reorders such a fragment independently of the label
    /// it belongs to, because the surrounding spaces and pipes are neutral: the status bar showed
    /// "Poll: 1000 Hz | cycle 18 us" with the number, the unit and the separator scattered across
    /// the line. An isolate keeps the fragment in one piece and takes its direction from its own
    /// first strong character, so a Latin unit reads left to right and a Persian one right to left
    /// without the caller having to know which.
    /// </para>
    /// </summary>
    public static string Isolate(string value) =>
        string.IsNullOrEmpty(value) ? string.Empty : $"\u2068{value}\u2069";

    /// <summary>Looks up a string by key, falling back to English and then to the key itself.</summary>
    public string this[string key]
    {
        get
        {
            if (_language == "fa" && Persian.TryGetValue(key, out var fa))
            {
                return fa;
            }

            return Portuguese.GetValueOrDefault(key, key);
        }
    }

    /// <summary>
    /// Indexable façade used from XAML as <c>Text[key]</c>. Avalonia's compiled bindings cannot
    /// bind to the default indexer of a source object directly, so the lookup is exposed through
    /// a named property whose value is itself indexable.
    /// </summary>
    public LocalizedText Text { get; }

    /// <summary>Static lookup helper for code-behind use.</summary>
    public static string Get(string key) => Instance[key];

    private Strings() => Text = new LocalizedText(this);

    /// <summary>Read-only indexable view over the active language table.</summary>
    public sealed class LocalizedText : INotifyPropertyChanged
    {
        private readonly Strings _owner;

        internal LocalizedText(Strings owner) => _owner = owner;

        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Looks up a localised string by key.</summary>
        public string this[string key] => _owner[key];

        /// <summary>
        /// Tells every binding to re-read the indexer.
        /// <para>
        /// The notification has to be raised here rather than on <see cref="Strings"/>: XAML binds
        /// <c>Text[key]</c>, so the indexer belongs to this object, and since this instance is
        /// created once and never replaced, a notification on the owner never reached the captions
        /// that had already been resolved - they kept the previous language until the window was
        /// recreated, which is why switching language appeared to need a restart.
        /// </para>
        /// </summary>
        internal void RaiseAllChanged()
        {
            // "Item[]" is the name an indexer binding listens for; the empty name is the catch-all
            // that means "every property changed", which covers both binding implementations.
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }

    private static readonly Dictionary<string, string> Portuguese = new(StringComparer.Ordinal)
    {
        ["app.title"] = "Ander Flow Controle",
        ["app.name"] = "Ander Flow Controle",

        ["nav.general"] = "Geral",
        ["nav.controls"] = "Controles",
        ["nav.advanced"] = "Avançado",
        ["nav.about"] = "Sobre",

        ["tab.player"] = "Jogador",
        ["tab.advanced"] = "Avançado",

        ["player.connect"] = "Conectar controle",
        ["player.inputDevice"] = "Dispositivo de entrada",
        ["player.profile"] = "Perfil",
        ["player.controllerType"] = "Tipo de controle",
        ["player.any"] = "Qualquer",
        ["player.notConnected"] = "Desconectado",
        ["player.keyboardMouse"] = "Teclado e mouse",
        
        ["profile.none"] = "Nenhum",
        ["profile.newTitle"] = "Novo perfil",
        ["profile.prompt"] = "Digite um nome para o perfil.",
        ["profile.hint"] = "O nome deve excluir estes caracteres.",
        ["profile.errExists"] = "Já existe um perfil com esse nome.",
        ["profile.errChars"] = "O nome contém um caractere inválido.",
        ["profile.errReserved"] = "O Windows reserva esse nome. Escolha outro.",
        ["profile.errTooLong"] = "Use um nome de até 64 caracteres.",

        ["group.leftStick"] = "Analógico esquerdo",
        ["group.rightStick"] = "Analógico direito",
        ["group.clear"] = "Limpar os atalhos deste grupo",
        ["group.dpad"] = "Direcional",
        ["group.faceButtons"] = "Botões frontais",
        ["group.shoulders"] = "Botões superiores",
        ["group.shouldersL"] = "Botão superior esquerdo",
        ["group.shouldersR"] = "Botão superior direito",
        ["group.triggers"] = "Gatilhos",
        ["group.misc"] = "Outros",
        ["group.keyboardMouse"] = "Teclado e mouse",

        ["bind.up"] = "Cima",
        ["bind.down"] = "Baixo",
        ["bind.left"] = "Esquerda",
        ["bind.right"] = "Direita",
        ["bind.pressed"] = "Pressionado",
        ["bind.modifier"] = "Modificador",
        ["bind.range"] = "Faixa",
        ["bind.deadzone"] = "Zona morta",
        ["bind.notSet"] = "[sem atalho]",
        ["bind.pressKey"] = "[pressione uma tecla]",
        ["bind.back"] = "Voltar",
        ["bind.guide"] = "Guia",
        ["bind.start"] = "Iniciar",

        ["menu.clear"] = "Limpar",
        ["menu.invertAxis"] = "Inverter eixo",
        ["menu.toggle"] = "Alternar",
        ["menu.setThreshold"] = "Definir limite…",
        ["menu.swapWith"] = "Trocar com…",

        ["action.save"] = "Salvar",
        ["action.new"] = "Novo",
        ["action.delete"] = "Excluir",
        ["action.rename"] = "Renomear",
        ["action.refresh"] = "Atualizar",
        ["action.autoMap"] = "Mapear automaticamente",
        ["action.clearAll"] = "Limpar tudo",
        ["action.defaults"] = "Restaurar padrões",
        ["action.ok"] = "OK",
        ["action.cancel"] = "Cancelar",
        ["action.apply"] = "Aplicar",
        ["action.close"] = "Fechar",
        ["action.configure"] = "Configurar",
        ["action.identify"] = "Identificar",
        ["action.install"] = "Instalar",
        ["action.dismiss"] = "Dispensar",
        ["action.update"] = "Verificar atualizações",
        ["action.openLogs"] = "Abrir registros",
        ["action.openDataFolder"] = "Abrir pasta de dados",
        ["action.updateMappingDb"] = "Atualizar banco de mapeamentos",
        ["action.restartNow"] = "Reiniciar agora",
        ["action.releasePage"] = "Abrir página da versão",
        ["action.autoDetectAll"] = "Detectar todos",

        ["tray.open"] = "Abrir Ander Flow Controle",
        ["tray.enableOutput"] = "Ativar controles virtuais",
        ["tray.disableOutput"] = "Desativar controles virtuais",
        ["tray.exit"] = "Sair",
        ["tray.tooltip"] = "Ander Flow Controle",
        ["tray.phones"] = "Celulares (QR Code)",

        ["about.heading"] = "Ander Flow Controle",
        ["about.tagline"] = "Controles sem fio para Windows",
        ["about.version"] = "Versão",
        ["about.description"] = "Use celulares ou outros dispositivos como controles virtuais Xbox 360 ou DualShock 4. Pareie os celulares com um QR Code e atribua um dispositivo a cada jogador.",
        ["about.credits"] = "Créditos e componentes de terceiros",
        ["about.creditSdl"] = "SDL3 — leitura dos dispositivos de entrada (licença zlib)",
        ["about.creditViGEm"] = "ViGEmBus e ViGEm.NET — Nefarius Software Solutions (MIT e BSD-3)",
        ["about.creditHidHide"] = "HidHide — Nefarius Software Solutions (MIT)",
        ["about.creditAvalonia"] = "Avalonia UI (MIT)",
        ["about.creditSerilog"] = "Serilog (Apache-2.0)",
        ["about.creditToolkit"] = "CommunityToolkit.Mvvm (MIT)",
        ["about.creditDb"] = "Banco de mapeamentos da comunidade SDL_GameControllerDB (zlib)",
        ["about.disclaimer"] = "Edição em português por Ander Flow, baseada no UniPad. Consulte a licença MIT e os avisos incluídos no pacote.",
        ["about.dataFolder"] = "Pasta de dados",
        ["about.maintenance"] = "Manutenção",
        ["about.portable"] = "Portátil",
        ["about.appData"] = "AppData",

        ["opt.vibration"] = "Vibração",
        ["opt.emulateStick"] = "Simular analógico esquerdo com o direcional",
        ["opt.outputMode"] = "Modo do controle virtual",
        ["opt.hidePhysical"] = "Ocultar controles físicos (HidHide)",
        ["opt.startMinimized"] = "Iniciar minimizado na área de notificação",
        ["opt.minimizeOnClose"] = "Minimizar para a área de notificação ao fechar",
        ["opt.runAtStartup"] = "Executar ao iniciar o Windows",
        ["opt.pollRate"] = "Taxa de leitura",
        ["opt.theme"] = "Tema",
        ["opt.language"] = "Idioma",
        ["opt.verboseLogging"] = "Registros detalhados",
        ["opt.outputEnabled"] = "Controles virtuais ativados",
        ["opt.keyboardMouseEnabled"] = "Ativar entrada por teclado e mouse",
        ["opt.mouseSensitivity"] = "Sensibilidade do mouse",
        ["opt.mouseReturnSpeed"] = "Retorno ao centro",
        ["opt.mouseInvertY"] = "Inverter eixo Y do mouse",

        ["unit.seconds"] = "s",
        ["unit.hz"] = "Hz",
        ["unit.microseconds"] = "µs",

        ["status.emulatedDevices"] = "Dispositivos virtuais",
        ["status.other"] = "Outros",
        ["status.connected"] = "Conectados",
        ["status.controllers"] = "Controles",
        ["status.xinputSlot"] = "Posição no XInput",
        ["status.driverMissing"] = "Instale o ViGEmBus para usar os controles virtuais.",
        ["status.hidhideMissing"] = "O HidHide é opcional e ajuda a evitar entradas duplicadas de controles físicos.",
        ["status.vigemConnected"] = "ViGEmBus instalado e conectado",
        ["status.vigemUnreachable"] = "ViGEmBus instalado, mas indisponível",
        ["status.vigemMissing"] = "ViGEmBus ausente",
        ["status.vigemUnknown"] = "Estado do ViGEmBus desconhecido",
        ["status.hidHideInstalled"] = "HidHide instalado",
        ["status.hidHideMissing"] = "HidHide ausente (opcional)",
        ["status.hidHideUnknown"] = "Estado do HidHide desconhecido",
        ["status.hiddenDevices"] = "ocultos",
        ["status.pollRate"] = "Leitura",
        ["status.cycle"] = "Ciclo",
        ["status.devices"] = "Dispositivos",

        ["msg.autoMapped"] = "Mapeamento automático aplicado. Confira os botões.",
        ["msg.profileLoaded"] = "Perfil carregado",
        ["msg.inputInitFailed"] = "Falha ao iniciar a entrada",
        ["msg.deviceSetupFailed"] = "Falha ao configurar o dispositivo",
        ["msg.restartFailed"] = "Abra o Ander Flow Controle novamente.",
        ["msg.mappingDbInvalid"] = "O arquivo recebido parece inválido. O banco atual foi mantido.",
        ["msg.mappingDbUpdated"] = "Banco de mapeamentos atualizado. Reinicie o aplicativo para aplicar.",
        ["msg.mappingDbFailed"] = "Falha ao atualizar o banco de mapeamentos",
        ["msg.openPathFailed"] = "Falha ao abrir o caminho",
        ["msg.autoMapKeyboard"] = "Configuração padrão de teclado e mouse aplicada.",
        ["msg.autoMapPhone"] = "Configuração padrão do celular aplicada.",
        ["msg.autoMapSdl"] = "Mapeamento do banco SDL aplicado",
        ["msg.autoMapGuessed"] = "Mapeamento sugerido. Confira os botões.",
        ["msg.autoMapFailed"] = "Defina o mapeamento manualmente.",
        ["msg.allCleared"] = "Atalhos limpos e configurações padrão restauradas.",
        ["msg.applying"] = "Aplicando…",
        ["msg.applyFailed"] = "Falha ao aplicar as alterações",
        ["msg.outputEnabled"] = "Controles virtuais ativados.",
        ["msg.outputDisabled"] = "Controles virtuais desativados.",
        ["msg.profileCreated"] = "Perfil criado",
        ["msg.profileDeleted"] = "Perfil excluído",
        ["msg.profileProtected"] = "O perfil padrão está protegido contra exclusão.",
        ["msg.logLevelRestart"] = "Reinicie o aplicativo para aplicar o nível de registro.",
        ["msg.startupEntryFailed"] = "Falha ao atualizar a inicialização automática no Windows.",
        ["msg.profileSaved"] = "Perfil salvo.",
        ["msg.applied"] = "Alterações aplicadas.",
        ["msg.noDevice"] = "Selecione um dispositivo de entrada.",
        ["msg.xinputLimit"] = "O Windows oferece quatro posições XInput. Os jogadores 5 a 8 usam DualShock 4 e precisam de um jogo compatível com esse controle.",
        ["msg.antiCheat"] = "Alguns jogos com sistema antitrapaça podem bloquear controles virtuais.",
        ["msg.keyboardMouseRestart"] = "Reinicie o aplicativo para aplicar a mudança.",
        ["msg.keyboardMouseDoubleInput"] = "Se o teclado e o controle virtual responderem juntos, remova as teclas duplicadas no próprio jogo.",
        ["msg.keyboardPassthrough"] = "Se o teclado e o controle virtual responderem juntos, remova as teclas duplicadas no próprio jogo.",
        ["msg.restartRequired"] = "A mudança será aplicada após reiniciar.",
        ["msg.updateChecking"] = "Verificando se há uma versão mais recente…",
        ["msg.upToDate"] = "O aplicativo está atualizado.",
        ["msg.updateDownloading"] = "Baixando a versão",
        ["msg.updateInstalled"] = "Atualização instalada. Reinicie o aplicativo.",
        ["msg.updateFailed"] = "Falha na atualização",
        ["msg.driverDownloading"] = "Baixando o driver",
        ["msg.driverInstalling"] = "Instalando o driver",
        ["msg.driverInstallingElevated"] = "Confirme o pedido de instalação do driver no Windows",
        ["msg.driverInstalled"] = "Driver instalado",
        ["msg.driverInstalledRestart"] = "Driver instalado. Reinicie o Windows",
        ["msg.driverCancelled"] = "Instalação do driver cancelada",
        ["msg.driverNotSupported"] = "A instalação do driver requer Windows.",
        ["msg.driverNotBundled"] = "Instale o driver manualmente",
        ["msg.driverDownloadIncomplete"] = "O arquivo baixado parece incompleto. Instale manualmente",
        ["msg.driverInstallerMissing"] = "Instalador ausente",
        ["msg.driverTimedOut"] = "A instalação ultrapassou o tempo limite",
        ["msg.driverInstallFailed"] = "Falha ao instalar o driver",
        ["msg.playerProfileSaved"] = "Perfil salvo",
        ["msg.playerProfileLoaded"] = "Perfil aplicado ao jogador",
        ["msg.playerProfileSaveFailed"] = "Falha ao salvar o perfil",
        ["msg.playerProfileLoadFailed"] = "Falha ao carregar o perfil",
        ["msg.playerProfileDeleteFailed"] = "Falha ao excluir o perfil",
        ["msg.phonePortChanged"] = "A porta estava ocupada. O servidor agora usa a porta",
        ["msg.phoneServerFailed"] = "Falha ao iniciar o servidor dos celulares",

        ["nav.phones"] = "Celulares",
        ["unit.ms"] = "ms",
        ["phones.connect"] = "Conectar um celular",
        ["phones.hint"] = "Conecte o PC e os celulares à mesma rede. Escaneie o QR Code com a câmera de cada celular.",
        ["phones.joinUrl"] = "Link de pareamento",
        ["phones.copy"] = "Copiar link",
        ["phones.copied"] = "Link copiado.",
        ["phones.newQr"] = "Gerar QR Code",
        ["phones.newQrHint"] = "Um novo código invalida o anterior. Os aparelhos conectados continuam funcionando e precisarão escanear o novo código se perderem a conexão.",
        ["phones.newQrDone"] = "Novo QR Code gerado. O código anterior expirou.",
        ["phones.server"] = "Servidor",
        ["phones.serverRunning"] = "Pareamento disponível na porta",
        ["phones.serverStopped"] = "Servidor indisponível. Verifique os registros para saber se outro programa está usando a porta.",
        ["phones.openAdmin"] = "Abrir no navegador",
        ["phones.connected"] = "Celulares conectados",
        ["phones.online"] = "Conectado",
        ["phones.offline"] = "Desconectado",
        ["phones.total"] = "Total",
        ["phones.colName"] = "Nome",
        ["phones.colStatus"] = "Estado",
        ["phones.colLatency"] = "Latência",
        ["phones.none"] = "Escaneie o QR Code para adicionar o primeiro celular.",
        ["phones.kick"] = "Desconectar",
        ["phones.kickConfirm"] = "Confirmar desconexão",
        ["phones.kickAll"] = "Desconectar todos",
        ["phones.kickAllConfirm"] = "Confirmar desconexão de todos",
        ["phones.kicked"] = "Celular desconectado",
        ["phones.allKicked"] = "Todos os celulares foram desconectados.",
        ["phones.renamed"] = "Celular renomeado",
    };

    private static readonly Dictionary<string, string> Persian = new(StringComparer.Ordinal)
    {
        ["app.title"] = "تنظیمات Ander Flow Controle",
        ["app.name"] = "Ander Flow Controle",

        ["nav.general"] = "عمومی",
        ["nav.controls"] = "کنترل‌ها",
        ["nav.advanced"] = "پیشرفته",
        ["nav.about"] = "درباره",

        ["tab.player"] = "بازیکن",
        ["tab.advanced"] = "پیشرفته",

        ["player.connect"] = "اتصال کنترلر",
        ["player.inputDevice"] = "دستگاه ورودی",
        ["player.profile"] = "پروفایل",
        ["player.controllerType"] = "نوع کنترلر",
        ["player.any"] = "هر کدام",
        ["player.notConnected"] = "متصل نیست",
        ["player.keyboardMouse"] = "کیبورد و ماوس",
        
        ["profile.none"] = "بدون پروفایل",
        ["profile.newTitle"] = "پروفایل جدید",
        ["profile.prompt"] = "نام پروفایل را وارد کنید:",
        ["profile.hint"] = "نام نمی‌تواند هیچ‌کدام از این نویسه‌ها را داشته باشد:",
        ["profile.errExists"] = "پروفایلی با این نام از قبل وجود دارد.",
        ["profile.errChars"] = "نام شامل نویسه‌ای است که مجاز نیست.",
        ["profile.errReserved"] = "این نام توسط ویندوز رزرو شده است. لطفاً نام دیگری انتخاب کنید.",
        ["profile.errTooLong"] = "نام بیش از حد طولانی است (حداکثر 64 نویسه).",

        ["group.leftStick"] = "آنالوگ چپ",
        ["group.rightStick"] = "آنالوگ راست",
        ["group.clear"] = "پاک کردن دکمه‌های این بخش",
        ["group.dpad"] = "دی‌پد",
        ["group.faceButtons"] = "دکمه‌های اصلی",
        ["group.shoulders"] = "دکمه‌های شانه",
        ["group.shouldersL"] = "شانه چپ",
        ["group.shouldersR"] = "شانه راست",
        ["group.triggers"] = "ماشه‌ها",
        ["group.misc"] = "متفرقه",
        ["group.keyboardMouse"] = "کیبورد و ماوس",

        ["bind.up"] = "بالا",
        ["bind.down"] = "پایین",
        ["bind.left"] = "چپ",
        ["bind.right"] = "راست",
        ["bind.pressed"] = "فشرده",
        ["bind.modifier"] = "تعدیل‌گر",
        ["bind.range"] = "بازه",
        ["bind.deadzone"] = "ناحیه مرده",
        ["bind.notSet"] = "[تنظیم نشده]",
        ["bind.pressKey"] = "[کلید را بزنید]",
        ["bind.back"] = "بازگشت",
        ["bind.guide"] = "راهنما",
        ["bind.start"] = "شروع",

        ["menu.clear"] = "پاک کردن",
        ["menu.invertAxis"] = "معکوس کردن محور",
        ["menu.toggle"] = "حالت ضامن‌دار",
        ["menu.setThreshold"] = "تنظیم آستانه...",
        ["menu.swapWith"] = "جابه‌جایی با...",

        ["action.save"] = "ذخیره",
        ["action.new"] = "جدید",
        ["action.delete"] = "حذف",
        ["action.rename"] = "تغییر نام",
        ["action.refresh"] = "بازخوانی",
        ["action.autoMap"] = "نگاشت خودکار",
        ["action.clearAll"] = "پاک کردن همه",
        ["action.defaults"] = "پیش‌فرض‌ها",
        ["action.ok"] = "تأیید",
        ["action.cancel"] = "انصراف",
        ["action.apply"] = "اعمال",
        ["action.close"] = "بستن",
        ["action.configure"] = "تنظیم",
        ["action.identify"] = "شناسایی",
        ["action.install"] = "نصب",
        ["action.dismiss"] = "بستن",
        ["action.update"] = "بررسی به‌روزرسانی",
        ["action.openLogs"] = "باز کردن پوشه لاگ",
        ["action.openDataFolder"] = "باز کردن پوشه داده",
        ["action.updateMappingDb"] = "به‌روزرسانی پایگاه‌داده نگاشت",
        ["action.restartNow"] = "راه‌اندازی مجدد",
        ["action.releasePage"] = "باز کردن صفحه انتشار",
        ["action.autoDetectAll"] = "شناسایی خودکار همه",

        ["tray.open"] = "باز کردن Ander Flow Controle",
        ["tray.enableOutput"] = "فعال کردن خروجی",
        ["tray.disableOutput"] = "غیرفعال کردن خروجی",
        ["tray.exit"] = "خروج",
        ["tray.tooltip"] = "Ander Flow Controle - نگاشت‌گر همه‌کاره کنترلر",
        ["tray.phones"] = "گوشی‌ها (QR)",

        ["about.heading"] = "Ander Flow Controle",
        ["about.tagline"] = "نگاشت‌گر همه‌کاره کنترلر برای ویندوز.",
        ["about.version"] = "نسخه",
        ["about.description"] =
            "Ander Flow Controle هر دستگاه ورودی — از گیم‌پدهای مدرن و دسته‌های قدیمی بدون آنالوگ تا استیک‌های آرکید، "
            + "فرمان‌ها و آداپتورهای USB بی‌نام — را به شکل یک کنترلر استاندارد Xbox 360 یا DualShock 4 به ویندوز "
            + "معرفی می‌کند، تا بازی‌هایی که فقط XInput را می‌شناسند با همان سخت‌افزاری که دارید کار کنند.",
        ["about.credits"] = "اجزای شخص ثالث",
        ["about.creditSdl"] = "SDL3 — شناسایی دستگاه و ورودی خام (مجوز zlib)",
        ["about.creditViGEm"] = "ViGEmBus / ViGEm.NET از Nefarius Software Solutions (MIT / BSD-3)",
        ["about.creditHidHide"] = "HidHide از Nefarius Software Solutions (MIT)",
        ["about.creditAvalonia"] = "Avalonia UI (MIT)",
        ["about.creditSerilog"] = "Serilog (Apache-2.0)",
        ["about.creditToolkit"] = "CommunityToolkit.Mvvm (MIT)",
        ["about.creditDb"] = "پایگاه‌داده نگاشت انجمنی SDL_GameControllerDB (Zlib)",
        ["about.disclaimer"] =
            "Ander Flow Controle یک شبیه‌ساز نیست و هیچ کد بازی در آن وجود ندارد. چیدمان رابط کاربری از پنجره‌های آشنای "
            + "تنظیم کنترلر الهام بصری گرفته است، اما تمام دارایی‌ها، استایل‌ها و خطوط کد این برنامه اثر اصیل خودش است.",
        ["about.dataFolder"] = "پوشه داده",
        ["about.maintenance"] = "نگهداری",
        ["about.portable"] = "قابل‌حمل",
        ["about.appData"] = "پوشهٔ کاربر",

        ["opt.vibration"] = "لرزش",
        ["opt.emulateStick"] = "شبیه‌سازی آنالوگ چپ با دی‌پد",
        ["opt.outputMode"] = "حالت خروجی",
        ["opt.hidePhysical"] = "مخفی کردن کنترلرهای فیزیکی (HidHide)",
        ["opt.startMinimized"] = "شروع به‌صورت مینیمایز در سینی",
        ["opt.minimizeOnClose"] = "مینیمایز به سینی هنگام بستن",
        ["opt.runAtStartup"] = "اجرا هنگام راه‌اندازی ویندوز",
        ["opt.pollRate"] = "نرخ خوانش",
        ["opt.theme"] = "پوسته",
        ["opt.language"] = "زبان",
        ["opt.verboseLogging"] = "لاگ کامل",
        ["opt.outputEnabled"] = "خروجی فعال",
        ["opt.keyboardMouseEnabled"] = "فعال‌سازی ورودی کیبورد و ماوس",
        ["opt.mouseSensitivity"] = "حساسیت ماوس",
        ["opt.mouseReturnSpeed"] = "زمان بازگشت به مرکز",
        ["opt.mouseInvertY"] = "معکوس کردن محور عمودی ماوس",

        ["unit.seconds"] = "ثانیه",
        ["unit.hz"] = "هرتز",
        ["unit.microseconds"] = "میکروثانیه",

        ["status.emulatedDevices"] = "دستگاه‌های شبیه‌سازی‌شده",
        ["status.other"] = "سایر",
        ["status.connected"] = "متصل",
        ["status.controllers"] = "کنترلرها",
        ["status.xinputSlot"] = "اسلات XInput",
        ["status.driverMissing"] = "درایور ViGEmBus نصب نیست. کنترلر مجازی در دسترس نیست.",
        ["status.hidhideMissing"] = "HidHide نصب نیست. برای بهترین نتیجه آن را نصب کنید تا ورودی دوتایی نشود.",
        ["status.vigemConnected"] = "ViGEmBus: نصب‌شده و متصل",
        ["status.vigemUnreachable"] = "ViGEmBus: نصب‌شده اما در دسترس نیست",
        ["status.vigemMissing"] = "ViGEmBus: نصب نشده",
        ["status.vigemUnknown"] = "ViGEmBus: وضعیت نامشخص",
        ["status.hidHideInstalled"] = "HidHide: نصب‌شده",
        ["status.hidHideMissing"] = "HidHide: نصب نشده (اختیاری اما توصیه می‌شود)",
        ["status.hidHideUnknown"] = "HidHide: وضعیت نامشخص",
        ["status.hiddenDevices"] = "دستگاه پنهان‌شده",
        ["status.pollRate"] = "نرخ خوانش",
        ["status.cycle"] = "چرخه",
        ["status.devices"] = "دستگاه‌ها",

        ["msg.autoMapped"] = "نگاشت خودکار انجام شد — لطفاً بررسی و اصلاح کنید.",
        ["msg.profileLoaded"] = "پروفایل بارگذاری شد:",
        ["msg.inputInitFailed"] = "راه‌اندازی ورودی ناموفق بود:",
        ["msg.deviceSetupFailed"] = "آماده‌سازی دستگاه ناموفق بود:",
        ["msg.restartFailed"] = "Ander Flow Controle دوباره اجرا نشد؛ لطفاً خودتان آن را اجرا کنید.",
        ["msg.mappingDbInvalid"] = "فایل پایگاه داده دانلودشده معتبر نیست؛ نسخه قبلی حفظ شد.",
        ["msg.mappingDbUpdated"] = "پایگاه داده نگاشت به‌روزرسانی شد. برای اعمال، برنامه را دوباره اجرا کنید.",
        ["msg.mappingDbFailed"] = "به‌روزرسانی پایگاه داده نگاشت ناموفق بود:",
        ["msg.openPathFailed"] = "این مسیر باز نشد:",
        ["msg.autoMapKeyboard"] = "چیدمان پیش‌فرض کیبورد و ماوس اعمال شد.",
        ["msg.autoMapPhone"] = "چیدمان استاندارد گوشی اعمال شد.",
        ["msg.autoMapSdl"] = "نگاشت از پایگاه داده کنترلرهای SDL انجام شد:",
        ["msg.autoMapGuessed"] = "نگاشت حدس زده شد؛ لطفاً بررسی و اصلاح کنید:",
        ["msg.autoMapFailed"] = "نگاشت حدس زده نشد؛ لطفاً دستی تعیین کنید:",
        ["msg.allCleared"] = "همهٔ نگاشت‌ها پاک و تنظیمات به پیش‌فرض بازگردانده شد.",
        ["msg.applying"] = "در حال اعمال...",
        ["msg.applyFailed"] = "اعمال ناموفق بود:",
        ["msg.outputEnabled"] = "خروجی فعال شد.",
        ["msg.outputDisabled"] = "خروجی غیرفعال شد.",
        ["msg.profileCreated"] = "پروفایل ساخته شد:",
        ["msg.profileDeleted"] = "پروفایل حذف شد:",
        ["msg.profileProtected"] = "پروفایل پیش‌فرض را نمی‌توان حذف کرد.",
        ["msg.logLevelRestart"] = "تغییر سطح لاگ پس از راه‌اندازی مجدد اعمال می‌شود.",
        ["msg.startupEntryFailed"] = "ثبت اجرای خودکار در ویندوز به‌روزرسانی نشد.",
        ["msg.profileSaved"] = "پروفایل ذخیره شد.",
        ["msg.applied"] = "تغییرها اعمال شد.",
        ["msg.noDevice"] = "ابتدا یک دستگاه ورودی انتخاب کنید.",
        ["msg.xinputLimit"] = "ویندوز فقط ۴ اسلات XInput دارد. بازیکنان ۵ تا ۸ از خروجی DualShock 4 استفاده می‌کنند که بازی‌های صرفاً XInput آن‌ها را نمی‌بینند.",
        ["msg.antiCheat"] = "بازی‌هایی با آنتی‌چیت کرنل‌مود ممکن است کنترلر مجازی را نپذیرند. این ابزار برای دور زدن هیچ محافظتی طراحی نشده است.",
        ["msg.keyboardMouseRestart"] = "برای اعمال این تغییر Ander Flow Controle را دوباره اجرا کنید.",
        ["msg.keyboardMouseDoubleInput"] = "کلیدهای فشرده‌شده علاوه بر پد مجازی به خود بازی هم می‌رسند. اگر هر دو هم‌زمان واکنش نشان دادند، کنترل‌های کیبورد را داخل بازی بردارید.",
        ["msg.keyboardPassthrough"] =
            "کلیدها هم‌زمان به بازی و به دسته مجازی می‌رسند. اگر هر دو با هم واکنش دادند، کنترل‌های "
            + "کیبورد را داخل خود بازی آزاد کنید.",
        ["msg.restartRequired"] = "این تغییر پس از راه‌اندازی مجدد اعمال می‌شود.",
        ["msg.updateChecking"] = "در حال بررسی نسخه جدید در گیت‌هاب...",
        ["msg.upToDate"] = "Ander Flow Controle به‌روز است",
        ["msg.updateDownloading"] = "دانلود نسخه",
        ["msg.updateInstalled"] = "به‌روزرسانی نصب شد — برنامه را دوباره اجرا کنید",
        ["msg.updateFailed"] = "به‌روزرسانی ناموفق بود",
        ["msg.driverDownloading"] = "در حال دانلود درایور:",
        ["msg.driverInstalling"] = "در حال نصب درایور:",
        ["msg.driverInstallingElevated"] = "در حال نصب درایور؛ پیام ویندوز را تأیید کنید:",
        ["msg.driverInstalled"] = "درایور نصب شد:",
        ["msg.driverInstalledRestart"] = "درایور نصب شد؛ راه‌اندازی مجدد لازم است:",
        ["msg.driverCancelled"] = "نصب درایور لغو شد:",
        ["msg.driverNotSupported"] = "نصب درایور فقط در ویندوز پشتیبانی می‌شود.",
        ["msg.driverNotBundled"] = "نصب‌کننده همراه این نسخه نیست؛ لطفاً دستی نصب کنید:",
        ["msg.driverDownloadIncomplete"] = "فایل دانلودشده ناقص به نظر می‌رسد؛ لطفاً دستی نصب کنید:",
        ["msg.driverInstallerMissing"] = "فایل نصب‌کننده پیدا نشد:",
        ["msg.driverTimedOut"] = "نصب‌کننده در زمان مقرر تمام نشد:",
        ["msg.driverInstallFailed"] = "نصب درایور ناموفق بود:",
        ["msg.playerProfileSaved"] = "پروفایل ذخیره شد:",
        ["msg.playerProfileLoaded"] = "پروفایل روی این بازیکن اعمال شد:",
        ["msg.playerProfileSaveFailed"] = "ذخیرهٔ پروفایل ممکن نشد:",
        ["msg.playerProfileLoadFailed"] = "خواندن پروفایل ممکن نشد:",
        ["msg.playerProfileDeleteFailed"] = "حذف پروفایل ممکن نشد:",
        ["msg.phonePortChanged"] = "پورت گوشی اشغال بود، سرور روی این پورت اجرا شد:",
        ["msg.phoneServerFailed"] = "سرور گوشی اجرا نشد:",

        ["nav.phones"] = "گوشی‌ها",
        ["unit.ms"] = "ms",
        ["phones.connect"] = "اتصال گوشی",
        ["phones.hint"] = "این QR را با دوربین گوشی اسکن کنید. گوشی و این کامپیوتر باید روی یک شبکهٔ Wi-Fi باشند.",
        ["phones.joinUrl"] = "لینک اتصال",
        ["phones.copy"] = "کپی لینک",
        ["phones.copied"] = "لینک در کلیپ‌بورد کپی شد.",
        ["phones.newQr"] = "ساخت QR جدید",
        ["phones.newQrHint"] = "QR جدید، QR قبلی را باطل می‌کند. گوشی‌های متصل وصل می‌مانند، اما اگر قطع شوند باید دوباره اسکن کنند.",
        ["phones.newQrDone"] = "QR جدید ساخته شد؛ QR قبلی دیگر کار نمی‌کند.",
        ["phones.server"] = "سرور",
        ["phones.serverRunning"] = "سرور گوشی روی این پورت فعال است:",
        ["phones.serverStopped"] = "سرور گوشی اجرا نشده است. ممکن است پورت توسط برنامهٔ دیگری اشغال باشد؛ جزئیات در لاگ است.",
        ["phones.openAdmin"] = "باز کردن در مرورگر",
        ["phones.connected"] = "گوشی‌های متصل",
        ["phones.online"] = "آنلاین",
        ["phones.offline"] = "قطع",
        ["phones.total"] = "کل",
        ["phones.colName"] = "نام",
        ["phones.colStatus"] = "وضعیت",
        ["phones.colLatency"] = "تأخیر",
        ["phones.none"] = "هنوز گوشی‌ای وصل نشده است. با گوشی‌ای که روی همین Wi-Fi است QR را اسکن کنید.",
        ["phones.kick"] = "اخراج",
        ["phones.kickConfirm"] = "تأیید اخراج",
        ["phones.kickAll"] = "قطع همه",
        ["phones.kickAllConfirm"] = "تأیید: قطع همه",
        ["phones.kicked"] = "گوشی قطع شد:",
        ["phones.allKicked"] = "همهٔ گوشی‌ها قطع شدند.",
        ["phones.renamed"] = "نام گوشی تغییر کرد:",
    };
}
