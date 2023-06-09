# Проект 2 - Информационная система (база данных)

[ТЗ для “Проект 2”](https://www.notion.so/2-6657117de4be40b0b8b8a0e336cdf5d6)

- **Условия, обязательные для правильной работы программы**
    - Примеры входных файлов есть в папке examples (каждая строчка учитывается как отдельная запись включая самую первую)
    - Необходимо заполнить файл Data.xlsx в соответствии с примером или загрузить свой, также соответствующий примеру (пример заполнения файла в examples)
    - Необходимо заполнить файл Users.xlsx (папка data) в соответствии с примером или загрузить свой, также соответствующий примеру (пример заполнения файла в examples)
    - Файлы в папке data именуются в формате yyyy.MM.dd, если они выгружаются вручную, и соответствуют своим датам

Необходимо создать команду из 5 человек на любом языке, поддерживающим объектно-ориентированное программирование, для реализации консольной информационной системы с использованием объектно-ориентированной парадигмы программирования. Вы должны руководствоваться требованиями и ограничениями, описанными ниже, но предметную область, внешний вид, язык программирования и иерархию классов в коде определяете самостоятельно в рамках команды.

- **Для вдохновения можно пользоваться следующими источниками:**
    - [Информационная система на Википедии](https://ru.wikipedia.org/wiki/%D0%98%D0%BD%D1%84%D0%BE%D1%80%D0%BC%D0%B0%D1%86%D0%B8%D0%BE%D0%BD%D0%BD%D0%B0%D1%8F_%D1%81%D0%B8%D1%81%D1%82%D0%B5%D0%BC%D0%B0)
    - [Пример интерфейса](https://raw.githubusercontent.com/styczynski/waccgl/master/static/screenshot0.png)
    - [Ещё пример интерфейса](https://i.stack.imgur.com/QmnFJ.png)
- **Основные требования к проекту (уровень 1)**
    1. Реализация проекта в объектно-ориентированной парадигме.
    2. Реализация проекта в консоли с использованием текста и ASCII-графики.
    3. Реализация механики бронирования / записи на определенную дату / время чего-то / на что-то.
    4. Реализация хранения информации в базе данных (файловой, самописной).
    5. База данных должна поддерживать возможность записи, изменения, удаления данных, поиска данных по параметрам и чтения их с выдачей пользователю.
    6. Бронирование или запись как минимум трех видов сущностей, которые будут влиять на логику работы информационной системы.
    7. Реализация подсистемы интерфейса, включающей текстовое поле, поле ввода, кнопку действия и контейнер для элементов интерфейса (окно).
    8. Реализация двух ролей - пользователь и администратор информационной системы с соответствующим набором действий, определенных предметной областью и механикой бронирования / записи для нее.
    9. Действия пользователей должны логгироваться, а вводимые данные должны проверяться на валидность в соответствии с предметной областью и здравым смыслом.
    - В системе классов программы должны быть предусмотрены следующие крупные блоки:
        - Бизнес-логика (бронирование/запись, особенности и сущности предметной области).
        - Интерфейс (окно, текстовое поле, поле ввода, кнопка с действием, обработка клавиатуры и т. д.).
        - База данных (структура, запись, изменение, удаление данных, поиск по параметрам, выборка данных).
        - Пользователи и роли.
- **Дополнительные требования к проекту (уровень 2)**
    1. База данных должна работать быстро, для этого нужно реализовать индексы.
    2. В интерфейсе должны быть реализованы выпадающие пункты главного меню.
    3. Для реализации клиент-серверной модели следует использовать сокеты.
    4. Следует придерживаться принципов ООП и SOLID.
    5. Проект следует вести на Github.
- **Оценивание проекта**
    1. Он работает.
    2. Основные и дополнительные требования к проекту выполнены, хотя бы формально.
    3. Работа в команде. Задачи и блоки проекта разделены между участниками команды. История ведения задач закреплена (например, в Trello или Kaiten). Каждый может рассказать про свои задачи и результаты по ним.
    4. Полная оценка за проект: 30 баллов. За каждое нереализованное требование начисляется штраф в минус 2 балла.
    5. Срок: с 24 по 28 апреля в соответствии с расписанием подгрупп. Далее штраф -2 балла за каждую неделю просрочки. Можно уйти в минус.
    6. Если проект сдан в срок, но не все требования выполнены, может быть дано еще 2 недели, но за них уже будет начислено по 1 баллу, а не по 2.
    7. Если проект реализован не на ООП, то начисляется 0 баллов за проект.
    8. Баллы оценивания могут изменяться, но принципы останутся прежними.
