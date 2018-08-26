import { Disposable, CancellationToken } from "vscode";
import { ChildProcess, spawn, SpawnOptions, exec } from "child_process";
import * as path from "path";
import * as fs from "fs";
import {
  ICommandResult,
  ICommand,
  IExecutionCommand,
  createDeferred,
  IDefinitionResult,
  CommandType
} from "./proxy";
import { UI, getUI } from "../common/ui";

export class VdfProxy implements Disposable {
  private proc?: ChildProcess;
  private workspacePath: string;
  private indexPath: string;
  private languageServerStarted: boolean;
  private cmdId: number = 0;
  private previousData = "";
  private commands = new Map<number, IExecutionCommand<ICommandResult>>();
  private commandQueue: number[] = [];
  private ui: UI;

  constructor(private extensionRootDir: string, workspacePath: string) {
    this.workspacePath = workspacePath;
    this.indexPath = this.getIndexPath();
    this.ui = getUI();
    this.startLanguageServer();
  }

  private async startLanguageServer(): Promise<void> {
    //TODO: Do any initializing
    return this.restartLanguageServer();
  }

  private restartLanguageServer(): Promise<void> {
    this.killProcess();
    this.clearPendingRequests();
    this.ui.IsUpdatingIndex = true;
    return this.initialize();
  }

  private initialize(): Promise<void> {
    let exePath = path.join(
      this.extensionRootDir,
      // TEMP: Final output will be in the resources folder
      "src/server/VDFServer/VDFServer/bin/Release/netcoreapp2.1/VDFServer.dll"
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
    var proc = spawn("dotnet", [exePath, indexPath, workspacePath]);

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

    proc.stderr.on("data", data => {
      console.log("Error Data: ", data.toString());
    });

    proc.stdout.on("data", stream => {
      const response: string = stream.toString();
      console.log("Response Data: ", response);

      if (this.responseIsResult(response)) {
        try {
          let result: ICommandResult = JSON.parse(response);
          if (result.requestId != null && this.commands.has(result.requestId)) {
            const cmd = this.commands.get(result.requestId);
            if (cmd) {
              this.commands.delete(result.requestId);

              const index = this.commandQueue.indexOf(cmd.id);
              if (index !== -1) {
                this.commandQueue.splice(index, 1);
              }

              if (cmd.token.isCancellationRequested) {
                this.tryResolve(cmd, undefined);
              } else {
                this.tryResolve(cmd, result);
              }
            }
          }
        } catch {}
      }

      this.clearPendingRequests();
      return;
    });

    this.proc = proc;
  }

  private clearPendingRequests() {
    const items = this.commandQueue.splice(0, this.commandQueue.length);
    items.forEach(id => {
      if (this.commands.has(id)) {
        const cmd = this.commands.get(id);
        this.tryResolve(cmd, undefined);
        this.commands.delete(id);
      }
    });
  }

  private responseIsResult(response: string): boolean {
    if (
      response === "TAG_NOT_FOUND" ||
      response === "NO_PROVIDER_FOUND" ||
      response === "LANGUAGE_SERVER_INDEXING"
    )
      return false;

    if (response === "LANGUAGE_SERVER_INDEXING_COMPLETE") {
      this.ui.IsUpdatingIndex = false;
      console.log(response);
      return false;
    }

    return true;
  }

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

    return userPrefPath;
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
      return Promise.reject(new Error("VDF Language Server not initialized"));
    }

    const executionCmd = <IExecutionCommand<T>>cmd;
    const payload = this.createPayload(executionCmd);

    this.commands.set(executionCmd.id, executionCmd);
    this.commandQueue.push(executionCmd.id);
    executionCmd.deferred = createDeferred<T>();

    try {
      this.proc.stdin.write(`${JSON.stringify(payload)}\n`);
      this.commands.set(executionCmd.id, executionCmd);
      this.commandQueue.push(executionCmd.id);
    } catch (ex) {
      console.error(ex);
      //If 'This socket is closed.' that means process didn't start at all (at least not properly).
      if (ex.message === "This socket is closed.") {
        this.killProcess();
      }

      return Promise.reject(ex);
    }

    return executionCmd.deferred.promise;
  }

  private createPayload<T extends ICommandResult>(
    cmd: IExecutionCommand<T>
  ): any {
    const payload = {
      id: cmd.id,
      prefix: "",
      lookup: cmd.command,
      possibleWord: cmd.possibleWord,
      path: cmd.fileName,
      source: cmd.source,
      workspacePath: this.workspacePath,
      line: cmd.lineIndex,
      column: cmd.columnIndex
    };

    return payload;
  }

  public dispose() {
    this.killProcess();
  }

  private tryResolve(
    command: IExecutionCommand<ICommandResult> | undefined | null,
    result: ICommandResult | PromiseLike<ICommandResult> | undefined
  ): void {
    if (command && command.deferred) {
      command.deferred.resolve(result);
    }
  }
}
