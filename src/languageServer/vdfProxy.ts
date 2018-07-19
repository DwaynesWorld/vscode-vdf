import { Disposable } from "vscode";
import { ChildProcess, spawn, SpawnOptions } from "child_process";
import * as path from "path";
import * as fs from "fs";
import { ICommandResult, ICommand, IExecutionCommand } from "./proxy";

export class VdfProxy implements Disposable {
  private proc?: ChildProcess;
  private workspacePath: string;
  private indexPath: string;
  private languageServerStarted: boolean;
  private cmdId: number = 0;
  private previousData = "";
  private commands = new Map<number, IExecutionCommand<ICommandResult>>();
  private commandQueue: number[] = [];

  constructor(private extensionRootDir: string, workspacePath: string) {
    this.workspacePath = workspacePath;
    this.indexPath = this.getIndexPath();
    this.startLanguageServer();
  }

  private async startLanguageServer(): Promise<void> {
    //TODO: Do any initializing

    return this.restartLanguageServer();
  }

  private restartLanguageServer(): Promise<void> {
    this.killProcess();
    this.clearPendingRequests();
    return this.initialize();
  }

  private initialize(): Promise<void> {
    let exePath = path.join(
      this.extensionRootDir,
      "resources/vdfLexer/bin/Debug/netcoreapp2.0/vdfLexer.dll"
    );

    return this.spawnProcess(exePath, this.workspacePath, this.indexPath).catch(
      ex => {
        this.languageServerStarted = false;
        //TODO: Add Error handling
      }
    );
  }

  private async spawnProcess(
    exePath: string,
    workspacePath: string,
    indexPath: string
  ) {
    var proc = spawn("dotnet", [exePath, workspacePath, indexPath]);

    proc.on("error", error => {
      console.log("Process error:", error);
    });

    proc.on("message", message => {
      console.log("Process message:", message);
    });

    proc.on("close", close => {
      console.log("Process close:", close);
    });

    proc.on("disconnect", disconnect => {
      console.log("Process disconnect:", disconnect);
    });

    proc.stdin.setDefaultEncoding("utf8");

    proc.stderr.on("data", function(data) {
      console.log(data.toString());
    });

    proc.stdout.on("data", function(data) {
      console.log(data.toString());
    });

    this.proc = proc;
  }

  // TODO: Add request queue
  private clearPendingRequests() {}

  private killProcess() {
    if (this.proc) {
      try {
        this.proc.kill();
      } catch (ex) {}
    }

    this.proc = undefined;
  }

  public getIndexPath(): string {
    let userPrefPath =
      process.env.APPDATA ||
      (process.platform == "darwin"
        ? process.env.HOME + "/Library/Preferences"
        : "/var/local");

    userPrefPath = path.join(userPrefPath, "vdf");

    if (!fs.existsSync(userPrefPath)) fs.mkdirSync(userPrefPath);

    return path.join(userPrefPath, "index.vli");
  }

  public getNextCommandId(): number {
    const result = this.cmdId;
    this.cmdId += 1;
    return result;
  }

  public async sendCommand<T extends ICommandResult>(
    cmd: ICommand<T>
  ): Promise<any> {
    if (!this.proc) {
      return Promise.reject(new Error("VdfLexer process not initialized"));
    }

    console.log(cmd);

    var message = { type: "Greeting", data: "Hello, World!" };
    console.log("Sending: ", JSON.stringify(message));
    this.proc.stdin.write(JSON.stringify(message) + "\n");

    return Promise.resolve("Success!");
  }

  private createPayload<T extends ICommandResult>(
    cmd: IExecutionCommand<T>
  ): any {}

  dispose() {
    this.killProcess();
  }
}
