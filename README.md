# FileFlow
Красивая и функциональная замена проводнику Windows, разработанная для ускорения и упрощения работы над другими проектами.

<picture>
 <img alt="YOUR-ALT-TEXT" src="https://github.com/REDIZIT/FileFlow/blob/main/.github/FileFlowPreview.png?raw=true">
</picture>

# Функционал


## Проекты
Из папки можно создать так называемый "Проект", который будет отображаться над дисками в сайдбаре.

Проект позволяет переходить к любой папки внутри этого проекта через окно Быстрого перехода (GoTo).

Это полезно когда есть много папок, ты помнишь как она примерно называется, но не помнишь где она находиться или тебе лень её искать.

Создать проект - ` Папка + ПКМ -> Создать проект ` 

Удалить проект - ` Папка + ПКМ -> Удалить проект `
Папка удалена не будет! Просто отключится индексация.


## Предпросмотр
Наведя на иконку картинки или звука появится окно предпросмотра/прослушивания.


## Уведомления о загрузках
Когда в стандартной папке загрузок появляется новый файл, то слева-снизу появляется уведомление.

` ПКМ ` - скрыть

` ЛКМ x2 ` - открыть

` Зажать и перетащить ` - переместить файл


## Быстрый переход (GoTo)
Позволяет быстро переходить к разным папкам не добавляя их в избранное.

` Левый shift x2 ` - открыть окно

` Стрелочки вверх и вниз ` - перемещение *(клик мышью пока не работает)*


Какие файлы и папки индексируются:
- Папки проектов
- Папки и файлы из закладок
- Папки и файлы внутри текущей папки
- Системные папки (AppData, Roaming, Local, LocalLow, Downloads, Videos, Programs, Startup, User)

Под User папкой я имею ввиду папку пользователя. Название этой папки разное для каждого пользователя. Найти имя пользователя можно в меню Пуск.


## Архивы
Работа с архивами внутри самого проводника. Можно просто кликнуть по архиву и перейти внутрь него как в обычную папку.
Также можно создавать архивы из папок и файлов, но только в формате **.zip**

Архивы с паролем пока не поддерживаются.


## Маркеры
FileFlow отслеживает изменение файловой системы чтобы пользователь мог понять какой из множества файлов был только что добавлен, а какой изменен.

Когда элемент был добавлен - возле него появляется синий кружок
Когда элемент был изменен - оранжевый кружок

` F5 ` - обновить текущую папку и сбросить маркеры

FileFlow также отслеживает изменения подпапок. Если был создан/удален/изменен файл или папка в подпапке, то эта подпапка помечается как измененная.


## Копирование файлов
При выборе элементов для копирования/вырезания они помечаются плашкой To copy/To cut.

Если пользователь скопировал файлы, но вставил в приложение, где это недопустимо, то вставится текст в виде путей до этих файлов.
Например, если пользователь скопировал файлы и попытался вставить их в текстовом редакторе.

` Escape ` - сбросить копирование/вырезание


## Закладки
Чтобы добавить в закладки нужно просто перетащить элемент в сайдбар.
В закладки можно добавить **и папки, и файлы**.


## Мгновенный запуск
Для мгновенного запуска по нажатии по иконке в панели задач, FileFlow продолжает работу в фоне при закрытии самого окна. Для того, чтобы полностью завершить работу нужно нажать кнопку выхода слева-снизу в сайдбаре (рядом с кнопкой настроек)


## Технические пути
Путь до crash логов: 
`<app location>/CrashLogs`

Путь до временной папки архивов: 
`<app location>/Temp/Archives`

Путь до файла настроек: 
`<user>/AppData/Roaming/FileFlow/settings.json`

Путь до папки корзины:
`<user>/AppData/Roaming/FileFlow/RecycleBin`


## Полезные горячие клавиши
` F5 ` - обновить текущую папку и очистить маркеры

` F2 ` - переименовать выделенный элемент

` Левый shift x2 ` - открыть Быстрый переход (GoTo)

` Escape ` - сбросить копирование/вырезание элементов

` Ctrl + D ` - продублировать выделенный элемент *(авто. скопировать + переименовать)*

` XMouse 1 ` - история: назад

` XMouse 2 ` - история: вперед

` СКМ (по папке) ` - открыть папку в новой вкладке

 `СКМ (по вкладке) ` - закрыть вкладку
 
