﻿using ePizzaHub.Entities;
using ePizzaHub.Services.Interfaces;
using ePizzaHub.WebUI.Interfaces;
using ePizzaHub.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ePizzaHub.WebUI.Areas.Admin.Controllers
{
    public class ItemController : BaseController
    {
        ICatalogService _catalogService;
        IFileHelper _fileHelper;

        public ItemController(ICatalogService catalogService, IFileHelper fileHelper)
        {
            _catalogService = catalogService;
            _fileHelper = fileHelper;
        }
        public IActionResult Index()
        {
            var data = _catalogService.GetItems();
            return View(data);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = _catalogService.GetCategories();
            ViewBag.ItemTypes = _catalogService.GetItemTypes();
            return View();
        }

        [HttpPost]
        public IActionResult Create(ItemModel model)
        {
            try
            {
                model.ImageUrl = _fileHelper.UploadFile(model.File);
                Item data = new Item
                {
                    Name = model.Name,
                    UnitPrice = model.UnitPrice,
                    CategoryId = model.CategoryId,
                    ItemTypeId = model.ItemTypeId,
                    Description = model.Description,
                    ImageUrl = model.ImageUrl
                };
                _catalogService.AddItem(data);
                return RedirectToAction("Index");
            }
            catch (Exception)
            {

            }
            ViewBag.Categories = _catalogService.GetCategories();
            ViewBag.ItemTypes = _catalogService.GetItemTypes();
            return View();
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Categories = _catalogService.GetCategories();
            ViewBag.ItemTypes = _catalogService.GetItemTypes();
            Item data = _catalogService.GetItem(id);
            ItemModel model = new ItemModel
            {
                Id = data.Id,
                Name = data.Name,
                UnitPrice = data.UnitPrice,
                CategoryId = data.CategoryId,
                ItemTypeId = data.ItemTypeId,
                Description = data.Description,
                ImageUrl = data.ImageUrl
            };
            return View("Create", model);
        }

        [HttpPost]
        public IActionResult Edit(ItemModel model)
        {
            try
            {
                if (model.File != null)
                {
                    _fileHelper.DeleteFile(model.ImageUrl);
                    model.ImageUrl = _fileHelper.UploadFile(model.File);
                }

                Item data = new Item
                {
                    Id = model.Id,
                    Name = model.Name,
                    UnitPrice = model.UnitPrice,
                    CategoryId = model.CategoryId,
                    ItemTypeId = model.ItemTypeId,
                    Description = model.Description,
                    ImageUrl = model.ImageUrl
                };

                _catalogService.UpdateItem(data);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

            }
            ViewBag.Categories = _catalogService.GetCategories();
            ViewBag.ItemTypes = _catalogService.GetItemTypes();
            return View("Create", model);
        }

        [Route("~/Admin/Item/Delete/{id}/{url}")]
        public IActionResult Delete(int id, string url)
        {
            url = url.Replace("%2F", "/"); //replace to find the file

            _catalogService.DeleteItem(id);
            _fileHelper.DeleteFile(url);
            return RedirectToAction("Index");
        }
    }
}
