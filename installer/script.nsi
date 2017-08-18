OutFile "MangaDownloader_v1.0.exe"
InstallDir "C:\Program Files (x86)\Fiskelord\Manga Downloader"

Section
	SetOutPath $INSTDIR
	
	File "DotNetZip.dll"
	File "HtmlAgilityPack.dll"
	File "Manga Downloader.exe"
	
	CreateShortCut "$DESKTOP\Manga Downloader.lnk" "$INSTDIR\Manga Downloader.exe"
	
	CreateDirectory "$SMPROGRAMS\Manga Downloader"
	CreateShortCut "$SMPROGRAMS\Manga Downloader\Manga Downloader.lnk" "$INSTDIR\Manga Downloader.exe"
	CreateShortCut "$SMPROGRAMS\Manga Downloader\Uninstall.lnk" "$INSTDIR\uninstaller.exe"
	
	WriteUninstaller "$INSTDIR\uninstaller.exe"
SectionEnd

Section "Uninstall"
	Delete "$INSTDIR\uninstaller.exe"
	Delete "$INSTDIR\DotNetZip.dll"
	Delete "$INSTDIR\HtmlAgilityPack.dll"
	Delete "$INSTDIR\Manga Downloader.exe"
	
	Delete "$DESKTOP\Manga Downloader.lnk"
	
	Delete "$SMPROGRAMS\Manga Downloader\Manga Downloader.lnk"
	Delete "$SMPROGRAMS\Manga Downloader\Uninstall.lnk"
	Delete "$SMPROGRAMS\Manga Downloader"
SectionEnd