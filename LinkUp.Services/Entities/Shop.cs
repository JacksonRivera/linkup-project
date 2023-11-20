using System;
using Amazon.DynamoDBv2.DataModel;
using LinkUp.Core.AWS.Business;
using LinkUp.Core.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace LinkUp.Shopify.Entities
{
    /// <summary>
    /// PartitionKey = AppId
    /// RowKey = shopId
    /// </summary>
    [DynamoDBTable("Shopify_App_Shops")]
    public class Shop : DynamoDBTableEntity
    {
        /// <summary>
        /// Dirección Mac del HOST
        /// </summary>
        public string HMAC { get; set; }
        /// <summary>
        /// Timestamp enviada por shopify
        /// </summary>
        public string ShopifyTimestamp { get; set; }
        /// <summary>
        /// Lista de permisos separados por coma(',')
        /// </summary>
        public string Scope { get; set; }
        /// <summary>
        /// Codigo de verificacion autenticidad respuestas Shopify
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// Codigo de verifación para obtener el token
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Token de acceso obligatorio para las solicitudes al api de shopify
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// url de confirmación de la subscripción al pago
        /// </summary>
        public string SubscriptionUrl { get; set; }
        /// <summary>
        /// Id de la subscripción al pago
        /// </summary>
        public string SubscriptionId { get; set; }
        /// <summary>
        /// Errores al crear la subscripción al pago
        /// </summary>
        public string SubscriptionErros { get; set; }
        /// <summary>
        /// Fue instalado? 
        /// </summary>
        public bool Installed { get; set; }
        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime CreationDate { get; set; }
        /// <summary>
        /// Fecha de instalacipon
        /// </summary>
        public DateTime? InstalationDate { get; set; }
    }
}
