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
    let self = this;

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
      console.log("Error Data: ", data.toString());
    });

    proc.stdout.on("data", function(stream) {
      const response = stream.toString();
      console.log("Out Data: ", response);

      let result: ICommandResult;

      try {
        result = JSON.parse(response);
      } catch {
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
        }
      }

      return;
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

  private parseDefinitionResult(result: any): IDefinitionResult | undefined {
    try {
      return JSON.parse(result) as IDefinitionResult;
    } catch {
      return;
    }
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
