import { Disposable } from "vscode";
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
    this.clearPendingRequests(this);
    return this.initialize();
  }

  private initialize(): Promise<void> {
    let exePath = path.join(
      this.extensionRootDir,
      "src/server/VDFServer/VDFServer/bin/Debug/netcoreapp2.1/VDFServer.dll"
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
    let self = this;

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

    proc.stderr.on("data", function(data) {
      console.log("Error Data: ", data.toString());
    });

    proc.stdout.on("data", function(stream) {
      // TODO: This is all trash, its somewhat working
      // But needs alot of work.
      const response: string = stream.toString();
      console.log("Response Data: ", response);

      if (response.indexOf("Indexing") !== -1) {
        self.clearPendingRequests(self);
      }

      let result: ICommandResult;

      try {
        result = JSON.parse(response);
      } catch {
        self.clearPendingRequests(self);
        return;
      }

      if (result.requestId != null && self.commands.has(result.requestId)) {
        const cmd = self.commands.get(result.requestId);
        if (cmd) {
          self.commands.delete(result.requestId);
          const index = self.commandQueue.indexOf(cmd.id);
          if (index !== -1) {
            self.commandQueue.splice(index, 1);
          }

          self.tryResolve(cmd, result);
          return;
        }
      }

      self.clearPendingRequests(self);
      return;
    });

    this.proc = proc;
  }

  // TODO: This is trash as well
  private clearPendingRequests(self: VdfProxy) {
    const items = self.commandQueue.splice(0, self.commandQueue.length);
    items.forEach(id => {
      if (self.commands.has(id)) {
        const cmd1 = self.commands.get(id);
        try {
          self.tryResolve(cmd1, undefined);
        } catch (ex) {
        } finally {
          self.commands.delete(id);
        }
      }
    });
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
