namespace ObjectMapping
{
    class Program
    {
        static void Main(string[] args)
        {
            // inModel
            var model = new Model("account_A", "name_B");

            // outModle
            FormDataObject formData = new FormDataObject("TagA");

            // Request
            var request = new Model();

            var objMapping = new ObjectMapping();

            var ans = objMapping.GetTreeMapResult<FormDataObject>(@"Setting\ModelToFromData.json", model, formData);

            request = objMapping.GetTreeMapResult<Model>(@"Setting\FromDataToModel.json", ans, request);
        }
    }


    class Model
    {
        public Model() { }

        public Model(string A, string B)
        {
            this.UserAccount = A;
            this.UserName = B;
        }

        public string SID { get; set; }

        public string UserAccount { get; set; }

        public string UserName { get; set; }
    }

    public class FormDataObject
    {
        public FormDataObject(string tag)
        {
            this.Tag = tag;
            this.FormMian = new FormMian();
            this.FormInfo = new FormInfo();
        }

        public FormMian FormMian { get; set; }

        public FormInfo FormInfo { get; set; }

        public string Tag { get; set; }
    }

    public class FormMian
    {
        public FormMian()
        {
            this.FormKey = "TEST018";
            this.FormID = "TEST01820220303TEST001";
        }

        public string FormKey { get; set; }

        public string FormID { get; set; }
    }

    public class FormInfo
    {
        public FormInfo()
        {
            this.Version = "V1";
        }

        public string Account { get; set; }

        public string Name { get; set; }

        public string Version { get; set; }
    }
}
