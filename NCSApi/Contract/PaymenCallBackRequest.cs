using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NCSApi.Contract
{
    public class PaymenCallBackRequest
    {
        // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
        public partial class ePaymentConfirmation
        {

            private string customsCodeField;

            private string declarantCodeField;

            private string bankCodeField;

            private ePaymentConfirmationSadAsmt sadAsmtField;

            private ePaymentConfirmationPayment paymentField;

            private decimal totalAmountToBePaidField;

            /// <remarks/>
            public string CustomsCode
            {
                get
                {
                    return this.customsCodeField;
                }
                set
                {
                    this.customsCodeField = value;
                }
            }

            /// <remarks/>
            public string DeclarantCode
            {
                get
                {
                    return this.declarantCodeField;
                }
                set
                {
                    this.declarantCodeField = value;
                }
            }

            /// <remarks/>
            public string BankCode
            {
                get
                {
                    return this.bankCodeField;
                }
                set
                {
                    this.bankCodeField = value;
                }
            }

            /// <remarks/>
            public ePaymentConfirmationSadAsmt SadAsmt
            {
                get
                {
                    return this.sadAsmtField;
                }
                set
                {
                    this.sadAsmtField = value;
                }
            }

            /// <remarks/>
            public ePaymentConfirmationPayment Payment
            {
                get
                {
                    return this.paymentField;
                }
                set
                {
                    this.paymentField = value;
                }
            }

            /// <remarks/>
            public decimal TotalAmountToBePaid
            {
                get
                {
                    return this.totalAmountToBePaidField;
                }
                set
                {
                    this.totalAmountToBePaidField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class ePaymentConfirmationSadAsmt
        {

            private string sADAssessmentSerialField;

            private string sADAssessmentNumberField;

            private ushort sADYearField;

            /// <remarks/>
            public string SADAssessmentSerial
            {
                get
                {
                    return this.sADAssessmentSerialField;
                }
                set
                {
                    this.sADAssessmentSerialField = value;
                }
            }

            /// <remarks/>
            public string SADAssessmentNumber
            {
                get
                {
                    return this.sADAssessmentNumberField;
                }
                set
                {
                    this.sADAssessmentNumberField = value;
                }
            }

            /// <remarks/>
            public ushort SADYear
            {
                get
                {
                    return this.sADYearField;
                }
                set
                {
                    this.sADYearField = value;
                }
            }
        }

        /// <remarks/>
        [System.SerializableAttribute()]
        [System.ComponentModel.DesignerCategoryAttribute("code")]
        [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
        public partial class ePaymentConfirmationPayment
        {

            private string meansOfPaymentField;

            private string referenceField;

            private decimal amountField;

            /// <remarks/>
            public string MeansOfPayment
            {
                get
                {
                    return this.meansOfPaymentField;
                }
                set
                {
                    this.meansOfPaymentField = value;
                }
            }

            /// <remarks/>
            public string Reference
            {
                get
                {
                    return this.referenceField;
                }
                set
                {
                    this.referenceField = value;
                }
            }

            /// <remarks/>
            public decimal Amount
            {
                get
                {
                    return this.amountField;
                }
                set
                {
                    this.amountField = value;
                }
            }
        }
    }
}
