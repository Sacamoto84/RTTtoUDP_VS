Конвертер с Jlink Telnet localhost:19021 в UDP Broadcast 192.168.0.255 на передачу 8888, прием на 8889.   
Есть возможность запуска Jlink и перезапуск контроллера через *.bat  
Папку Jlink кидаем на диск C:\

	SEGGER_RTT_WriteString(0, "033[01;03;38;05;226;48;05;24m");
	SEGGER_RTT_WriteString(0, "Start\x1B[0m\r\n");
    
!Важно Окончание строки \r\n, это требуют все терминалы
!Чтобы работали скрипты, нужно в PATH windows добавить папку где лежит jlink.exe
например C:\Program Files\SEGGER\JLink

В *.bat нужно указать свой контроллер чтобы был коннект

VS2022 -> Git
