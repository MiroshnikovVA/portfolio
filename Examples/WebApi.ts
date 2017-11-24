
//function WebApi() {
//    var WebApi = {
//        Get: function (url, callbackSuccess) {
//            jQuery(function ($) {
//                $.ajax({
//                    type: "GET",
//                    url: url,
//                    success: callback
//                });
//            })
//        }
//    }
//    return WebApi;
//}

interface IAmazonApplication {
    FindRequest: string,
    Bundle: string,
    Ours: boolean,
    Img: string
}
interface IBot {
    name: string;
    task: boolean;
    proxy_adb: boolean;
}
interface IHost {
    name: string;
    power_net: boolean;
    bots: IBot[];
}
interface IHostList {
    hosts: IHost[];
}

class WebApi{
    static async get(url): Promise<string> {
        return new Promise<string>((resolve, reject) => {
            var xhr = new XMLHttpRequest();

            xhr.onreadystatechange = function (event) {
                if (xhr.readyState !== 4) return;
                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(xhr.responseText);//OK
                } else {
                    reject(xhr.statusText);//Error
                }
            };
            xhr.open('GET', url, true);//Async
            xhr.send();
        });
    }

    static async post(url, content): Promise<string> {
        return new Promise<string>((resolve, reject) => {
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function (event) {
                if (xhr.readyState !== 4) return;
                if (xhr.status >= 200 && xhr.status < 300) {
                    resolve(xhr.responseText);//OK
                } else {
                    reject(xhr.statusText);//Error
                }
            };
            xhr.open('POST', url, true);//Async
            xhr.setRequestHeader('Content-Type', 'application/json;charset=UTF-8')
            xhr.send(JSON.stringify(content));
        });
    }

    static async getAppList(): Promise<IAmazonApplication[]> {
        return JSON.parse(await WebApi.get("/api/AmazonApps")) as IAmazonApplication[];
    }
    static async getHostList(): Promise<IHostList> {
        return JSON.parse(await WebApi.get("/api/Querry/Hosts")) as IHostList;
    }
    static async sendTasks(tasks: string[][]): Promise<boolean> {
        var content = {
            Data : tasks
        };
        var rez = (await WebApi.post("/api/Querry/GenerateTasks", content)) as string;
        var bRez = rez == "Task added.";
        if (!bRez)
            alert(rez);
        return bRez;
    }

    static async saveApplication(app: IAmazonApplication): Promise<boolean> {
        var rez = (await WebApi.post("/api/AmazonApps/Save", app)) as string;
        var bRez = rez == "Application saved.";
        if (!bRez)
            alert(rez);
        return bRez;
    }
     

    //static async sendSelectedApp(): Promise<string> {

    //}
}


