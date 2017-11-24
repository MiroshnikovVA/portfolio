class PopupWindow {
    parent: PopupWindow;

    show() {
        show(this.window);
        if (this.parent != null) hide(this.parent.window);
        this.window.className = this.window.className;
    }

    hide() {
        hide(this.window);
        if (this.parent != null) {
            show(this.parent.window);
            this.parent.window.className = this.parent.window.className;
       }
    }

    window: HTMLDivElement;

    constructor(window: HTMLDivElement) {
        this.window = window;
    }
}

class SelectAppWindow extends PopupWindow {

    public addTaskButton: HTMLElement;

    private onAppEditEventHandler: (appBundle: IAmazonApplication) => void = (appBundle) => { };
    private onAppRefreshEventHandler: (apps: ReadDictionary<string, IAmazonApplication>) => void = (apps) => { };

    private selectedApps: List<string> = new List<string>();
    private appListBox: HTMLDivElement;
    private imageAppOk: [HTMLImageElement] = {} as [HTMLImageElement];
    
    private static appButtonOncontextmenu(e, appBundle: string) {
        var _this2 = e.currentTarget.jsclass as SelectAppWindow;
        _this2.onAppEditEventHandler(_this2.allApps.get(appBundle));
        return false;
    }
    private static appButtonClick(e) {
        var _this2 = e.currentTarget.jsclass as SelectAppWindow;
        var img = e.currentTarget.getElementsByTagName("img")[1] as HTMLImageElement;
        _this2.onAppButtonClick(img, _this2.allApps.get(img.alt));
    }

    private onAppButtonClick(imgElement: HTMLImageElement, app: IAmazonApplication) {
        if (isHidden(imgElement)) {
            imgElement.className = "imageok";
            show(imgElement);
            this.selectedApps.add(app.Bundle);
        } else {
            this.selectedApps.remove(app.Bundle);
            hide(imgElement);
        }
        if (this.selectedApps.count() > 0) {
            this.imageAppOk[this.selectedApps.first()].className = "imageokMain";
        }
    }

    onAppEdit(event: (appBundle: IAmazonApplication) => void) {
        this.onAppEditEventHandler = event;
    }

    onAppRefresh(event: (apps: ReadDictionary<string, IAmazonApplication>) => void) {
        this.onAppRefreshEventHandler = event;
    }

    private allApps: ReadDictionary<string, IAmazonApplication>;

    public refreshSelectedApps() {
        for (var bundle in this.imageAppOk) {
            var img = this.imageAppOk[bundle] as HTMLImageElement;
            if (img != null) {
                hide(img);
            }
        }

        this.selectedApps.forEach((bandle, index) => {
            var img = this.imageAppOk[bandle] as HTMLImageElement;
            if (img != null) {
                img.className = (index == 0) ? "imageokMain" : "imageok";
                show(img);
            }
        });

    }

    public async refreshListBox() {
        try {
            var apps = await WebApi.getAppList();
            this.allApps = new ReadDictionary<string, IAmazonApplication>(apps, app => app.Bundle);
            this.onAppRefreshEventHandler(this.allApps);
            var htmlCodeInPanel = apps.map(app => `
            <div class="appbutton" onmousedown="SelectAppWindow.appButtonClick(event)" oncontextmenu="SelectAppWindow.appButtonOncontextmenu(event,'${app.Bundle}')" >
            <img class="image" src="${app.Img}" />
            <img name="imageok" alt="${app.Bundle}" class="imageok" src="images/install/u34.png" />
            </div>`).join("\n");
            this.appListBox.innerHTML = htmlCodeInPanel;
            var array = this.appListBox.getElementsByClassName("appbutton");
            for (var i = 0, l = array.length; i < l; i++) {
                var button = array[i];
                button["jsclass"] = this;
                var img = button.getElementsByTagName("img")[1] as HTMLImageElement;
                this.imageAppOk[img.alt] = img;
            }
            this.window.className = this.window.className;
        } catch (exception) {
            alert(exception);
        }
        this.refreshSelectedApps();
    }

    constructor(window: HTMLDivElement, appListBox: HTMLDivElement) {
        super(window);
        this.appListBox = appListBox;
        this.refreshListBox();
    }

    public getCurrentTaskAndClear() {
        var rez = this.selectedApps;
        this.selectedApps = new List<string>();
        return rez;
    }

    public setTask(task:List<string>) {
        this.selectedApps = task;
        this.refreshSelectedApps();
    }
}

class List<T> {

    constructor() { }

    public setArray(array: T[]) {
        this.array = array;
    }

    public getArray() {
        return this.array;
    }

    private array: T[] = [] as T[]; 

    public count() { return this.array.length;}

    public add(item: T) {
        if (this.array.find(b => b == item)) return;
        this.array.push(item);
    }

    public remove(item: T) {
        var index = this.array.indexOf(item);
        var newArray = [] as T[];
        this.array.forEach(oldItem => { if (oldItem != item) newArray.push(oldItem) });
        this.array = newArray;
    }

    public first() {
        return this.array.length > 0 ? this.array[0] : null;
    }

    public forEach(callback: (item: T, index: number) => void) {
        this.array.forEach((item, index) => callback(item, index));
    }

    public where(predicat: (item: T) => boolean) {
        var newArray = this.array.filter(value => predicat(value));
        var list = new List<T>();
        list.setArray(newArray);
        return list;
    }
}

class ReadDictionary<TKey, TValue> {

    private dic: [TValue] = {} as [TValue]; 

    constructor(array: TValue[], getKey: (value: TValue) => TKey) {
        array.forEach(value => {
            var key = getKey(value) as any;
            this.dic[key] = value;
        });
    }

    public get(key: TKey) { return this.dic[key as any]; }
}

class MakeTaskAppWindow extends PopupWindow {

    taskList: List<List<string>> = new List<List<string>>();

    private onTaskEditEventHandler: (apps: List<string>) => void = (apps) => { };	

    public onTaskEdit(callback: (apps: List<string>) => void) {
        this.onTaskEditEventHandler = callback;
    }

    public newTaskButton: HTMLElement;
    private allApps: ReadDictionary<string, IAmazonApplication>;
    public setAllApps(apps: ReadDictionary<string, IAmazonApplication>) {
        this.allApps = apps;
        this.refreshAll();
    }
    public addTask(task: List<string>) {
        if (task.count() > 0) {
            this.taskList.add(task);
        }
        this.refreshAll();
    }

    public generateTaskRequest() {
        var data = this.taskList
            .where((value) => { return value.count() > 0; }).getArray()
            .map((value) => { return value.getArray(); });
        if (WebApi.sendTasks(data)) {
            alert("Задачи сгенерированы");
        }
    }

    refreshAll() {
        this.taskList = this.taskList.where(task => task.count() > 0);
        this.taskList.add(new List<string>());
        $(this.window).children(".wrapPanel").children().remove();
        this.taskList.forEach(task => {
            var taskPanel = $(document.createElement("div")).addClass("taskPanel");
            $(this.window).children(".wrapPanel").append(taskPanel);
            var localTask = task;
            task.forEach((bundle, index) => {
                var app = this.allApps.get(bundle);
                var appbutton = $(document.createElement("div")).addClass("appbutton")
                    .on("click", () => {
                        
                    })
                    .on("contextmenu", () => {
                        localTask.remove(app.Bundle);
                        appbutton.hide(1000);
                        if (localTask.count() == 0) {
                            taskPanel.hide(1500);
                        }
                    });
                var appIcon = ($(document.createElement("img"))
                    .addClass("image")
                    .attr("src", app.Img));
                var okIcon = ($(document.createElement("img"))
                    .addClass("imageok")
                    .attr("src", "images/install/u157.png")
                    .attr("style","background-color:none"));
                appbutton
                    .append(appIcon)
                    .append(okIcon);
                taskPanel.append(appbutton);
            });
            (() => {
                var newTaskButton = $(document.createElement("div")).addClass("appbutton").on("click", () => {
                    this.taskList.remove(localTask);
                    this.onTaskEditEventHandler(localTask);
                });
                var imageTaskButton = $(document.createElement("img"))
                    .addClass("image")
                    .attr("style", "background-color:none")
                    .attr("src", "images/install/u157.png");
                newTaskButton.append(imageTaskButton);
                taskPanel.append(newTaskButton);
            })();
        });
    }
}

class HostEditWindow extends PopupWindow {
    public createTaskListButon: HTMLElement;
}

class AppEditWindow extends PopupWindow {

    findRequestInput: HTMLInputElement;
    bundleInput: HTMLInputElement;
    oursInput: HTMLInputElement;
    imgInput: HTMLInputElement;
    image: HTMLImageElement;
    app: IAmazonApplication;

    setApp(app: IAmazonApplication) {
        this.app = app;
        //alert(app.Bundle);
        this.findRequestInput.value = app.FindRequest;
        this.bundleInput.value = app.Bundle;
        this.oursInput.value = app.Ours ? "true" : "else";
        this.imgInput.value = app.Img;
        this.image.src = app.Img;
    }

    async trySave(): Promise<boolean> {
        var newFindRequest = this.findRequestInput.value;
        var newBundle = this.bundleInput.value;
        var newOurs = this.bundleInput.value == "true";
        var newImg = this.imgInput.value;
        var app = this.app;
        if (newFindRequest != app.FindRequest
            || newBundle != app.Bundle
            || newOurs != app.Ours
            || newImg != app.Img) {
            var newApp: IAmazonApplication = {
                Bundle: newBundle,
                FindRequest: newFindRequest,
                Img: newImg,
                Ours: newOurs
            };
            return await WebApi.saveApplication(newApp);
        }
        return false;
    }




    public completeEditButton: HTMLElement;
}

class HostsTab extends PopupWindow {

}

class ClientApp {
    constructor() {

    }

    private selectAppWindow: SelectAppWindow;
    private makeTaskAppWindow: MakeTaskAppWindow;
    private appEditWindow: AppEditWindow;
    private hostEditWindow: HostEditWindow;
    private hostsTab: HostsTab;
    selectAppWindowInit(window: HTMLDivElement, appListBox: HTMLDivElement, addTaskButton: HTMLElement) {
        if (window == null) alert("SelectAppWindowInit argument null exception");
        if (appListBox == null) alert("SelectAppWindowInit argument null exception");
        this.selectAppWindow = new SelectAppWindow(window, appListBox);
        this.selectAppWindow.hide();
        this.selectAppWindow.addTaskButton = addTaskButton;
        return this;
    }
    makeTaskAppWindowInit(window: HTMLDivElement, newTaskButton: HTMLElement) {
        if (window == null) alert("MakeTaskAppWindowInit argument null exception");
        this.makeTaskAppWindow = new MakeTaskAppWindow(window);
        this.makeTaskAppWindow.hide();
        this.makeTaskAppWindow.newTaskButton = newTaskButton;
        return this;
    }
    appEditWindowInit(window: HTMLDivElement, completeEditButton: HTMLElement) {
        if (window == null) alert("AppEditWindowInit argument null exception");
        this.appEditWindow = new AppEditWindow(window);
        this.appEditWindow.hide();
        this.appEditWindow.completeEditButton = completeEditButton;
        return this;
    }
    HostEditWindowInit(window: HTMLDivElement, createTaskListButon: HTMLElement) {
        if (window == null) alert("HostEditWindowInit argument null exception");
        this.hostEditWindow = new HostEditWindow(window);
        this.hostEditWindow.hide();
        this.hostEditWindow.createTaskListButon = createTaskListButon;
        return this;
    }
    HostsTabInit(window: HTMLDivElement) {
        if (window == null) alert("HostsTab argument null exception");
        this.hostsTab = new HostsTab(window);
        //this.hostsTab.hide();
        return this;
    }
    start() {
        function disablecontext(e) {
            var clickedEl = (e == null) ? event.srcElement.tagName : e.target.tagName;
            if (clickedEl == "IMG") {
                return false;
            }
        }
        this.makeTaskAppWindow.parent = this.hostEditWindow;
        this.selectAppWindow.parent = this.makeTaskAppWindow;
        this.appEditWindow.parent = this.selectAppWindow;

        this.appEditWindow.findRequestInput = (document.getElementById("findRequestFromEditableApplications") as HTMLInputElement);
        this.appEditWindow.bundleInput = (document.getElementById("bundleFromEditableApplications") as HTMLInputElement);
        this.appEditWindow.oursInput = (document.getElementById("oursFromEditableApplications") as HTMLInputElement);
        this.appEditWindow.imgInput = (document.getElementById("imgFromEditableApplications") as HTMLInputElement);
        this.appEditWindow.image = (document.getElementById("imageFromEditableApplications") as HTMLImageElement)

        //Buttons------------------
        $(this.hostEditWindow.createTaskListButon).on("click", () => {
            this.makeTaskAppWindow.show();
        });

        //$(this.makeTaskAppWindow.newTaskButton).on("click", () => {
        //    this.selectAppWindow.show();
        //});

        $(this.selectAppWindow.addTaskButton).on("click", () => {
            var newTask = this.selectAppWindow.getCurrentTaskAndClear();
            this.makeTaskAppWindow.addTask(newTask);
            this.selectAppWindow.hide();
        });

        $(this.appEditWindow.completeEditButton).on("click", async () => {
            this.appEditWindow.hide();
            if (await this.appEditWindow.trySave()) {

            }
            this.selectAppWindow.refreshListBox();
        });

        $(this.makeTaskAppWindow.window).children(".bottompanel").children(".buttonCenter").on("click", () => {
            this.makeTaskAppWindow.hide();
            this.makeTaskAppWindow.generateTaskRequest();
        });
        //-------------

        //events-------------
        this.makeTaskAppWindow.onTaskEdit((apps) => {
            this.selectAppWindow.show();
            this.selectAppWindow.setTask(apps);
        });

        this.selectAppWindow.onAppEdit((app) => {
            this.appEditWindow.setApp(app);
            this.appEditWindow.show();
        });

        this.selectAppWindow.onAppRefresh((apps) => {
            this.makeTaskAppWindow.setAllApps(apps);
        });
        //-----------

        //init
        this.hostEditWindow.show();
        document.oncontextmenu = disablecontext;
    }
}