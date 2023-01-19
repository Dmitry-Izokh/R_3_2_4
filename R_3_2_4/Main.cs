using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R_3_2_4
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

           // Создаю переменную фильтра по категории и отфильтровываю все стены и типы стен. Делаю это только один раз для обоих фильтров по этажам.
            ElementCategoryFilter wallCategoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            // Извлекаю все уникальные элементы по ID по следующим условиям:
            // 1. Нахожу уровень 1 (Только те элементы класс которых соответствует точно выбранному классу) задача найти элемент = Этаж 01 или 02.
            // 2. Где переменная "х" = level с именем точно соответствующем "Этаж 01" (посмотрел в LookUP)
            // 3. Возвращает единственный и первый найденный элемент соответствующий названию (первый элемент последовательности или элемент по умолчанию. Что это значит не понятно).
            ElementId level1Id = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Where(x => x.Name.Equals("Этаж 01"))
                .FirstOrDefault().Id;

            ElementId level2Id = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Where(x => x.Name.Equals("Этаж 02"))
                .FirstOrDefault().Id;

            //Создаю фильтр для сопоставления элементов по их связанному уровню. В данном случае прошу найти элементы привязанные к уровню 1.
            ElementLevelFilter level1Filter = new ElementLevelFilter(level1Id);
            // Создаю логический фильтр по двум условиям и||и где первым условием выступает переменная "все стены в документе" а вторым условием переменная отвечающая за выборку всех элементов привязанных к 1 этажу.
            LogicalAndFilter wallsFilter1 = new LogicalAndFilter(wallCategoryFilter, level1Filter);

            ElementLevelFilter level2Filter = new ElementLevelFilter(level1Id);
            LogicalAndFilter wallsFilter2 = new LogicalAndFilter(wallCategoryFilter, level2Filter);

            // Видимо типы стен отсеялись, так как они не могут быть привязаны к уровням.

            // Создаю переменную фильтра который будет искать и фильтровать набор элементов из выборки логического фильтра (по 1 этажу).
            // По существу этот фильтр просто занесет все результаты в коллекцию List
            // 1. Из результатов логического фильтра wallsFilter1
            // 2. Элементы указанного типа, в данном случае стены.
            // 3. Собираем все это в лист.
            var walls1 = new FilteredElementCollector(doc)
                .WherePasses(wallsFilter1)
                .Cast<Wall>()
                .ToList();

            // Вывожу результаты в диалоговое окно: Название окна, текст в окне, количество элементтов из финального списка по 1 этажу записанное в строку.
            TaskDialog.Show("Количество стен", $"Количество стен на 1 этаже: {walls1.Count.ToString()}");

            var walls2 = new FilteredElementCollector(doc)
                .WherePasses(wallsFilter2)
                .Cast<Wall>()
                .ToList();

            TaskDialog.Show("Количество стен", $"Количество стен на 2 этаже: {walls2.Count.ToString()}");
            return Result.Succeeded;
        }
    }
}
