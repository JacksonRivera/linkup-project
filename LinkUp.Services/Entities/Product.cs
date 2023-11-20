using System;
using LinkUp.Core.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace LinkUp.Shopify.Entities
{
    [TableName("Products")]
    public class Product: TableEntity
    {
        /// <summary>
        /// Id de la  tienda
        /// </summary>
        public string ProductId { get; set; }
        /// <summary>
        /// Id de la app
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// Id de la  tienda
        /// </summary>
        public string ShopId { get; set; }
        /// <summary>
        /// Nombre del producto + nombre variante
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Nombre del producto 
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// Nombre variante
        /// </summary>
        public string VariantName { get; set; }
        /// <summary>
        /// Imagen del producto
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// Handle identificador
        /// </summary>
        public string Handle { get; set; }
        /// <summary>
        /// Codígo del producto
        /// </summary>
        [JsonProperty(PropertyName = "SKU")]
        public string SKU { get; set; }
        /// <summary>
        /// Precio antes de descuento
        /// </summary>
        public double Price { get; set; }
        /// <summary>
        /// Precio comparativo antes de descuento
        /// </summary>
        public double ComparePrice { get; set; }
        /// <summary>
        /// Precio despues de descuento
        /// </summary>
        public double CurrentPrice { get; set; }
        /// <summary>
        /// Precio despues antes de descuento
        /// </summary>
        public double CurrentComparePrice { get; set; }
        /// <summary>
        /// El descuento esta habilitado?
        /// </summary>
        public bool Enable { get; set; }

        public string Status { get; set; }
    }
}

