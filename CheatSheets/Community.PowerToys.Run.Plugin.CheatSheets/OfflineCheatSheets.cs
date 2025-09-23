using System.Collections.Generic;
using System.Linq;

namespace Community.PowerToys.Run.Plugin.CheatSheets;

/// <summary>
/// Offline cheat sheets for essential commands when network is unavailable
/// </summary>
public static class OfflineCheatSheets
{
    private static readonly Dictionary<string, List<OfflineCheatSheetItem>> OfflineSheets = new()
    {
        ["git"] = new List<OfflineCheatSheetItem>
        {
            new("git status", "Show the working tree status", "git"),
            new("git add .", "Add all changes to staging", "git"),
            new("git add [file]", "Add specific file to staging", "git"),
            new("git commit -m \"message\"", "Commit staged changes with message", "git"),
            new("git push", "Push commits to remote repository", "git"),
            new("git pull", "Pull changes from remote repository", "git"),
            new("git clone [url]", "Clone a repository", "git"),
            new("git branch", "List all branches", "git"),
            new("git branch [name]", "Create new branch", "git"),
            new("git checkout [branch]", "Switch to branch", "git"),
            new("git merge [branch]", "Merge branch into current", "git"),
            new("git log", "Show commit history", "git"),
            new("git diff", "Show changes", "git"),
            new("git reset --hard HEAD", "Reset to last commit", "git"),
            new("git stash", "Stash current changes", "git"),
            new("git stash pop", "Apply stashed changes", "git")
        },

        ["docker"] = new List<OfflineCheatSheetItem>
        {
            new("docker ps", "List running containers", "docker"),
            new("docker ps -a", "List all containers", "docker"),
            new("docker run [image]", "Run a container", "docker"),
            new("docker run -it [image] bash", "Run container interactively", "docker"),
            new("docker stop [container]", "Stop container", "docker"),
            new("docker rm [container]", "Remove container", "docker"),
            new("docker rmi [image]", "Remove image", "docker"),
            new("docker build -t [name] .", "Build image from Dockerfile", "docker"),
            new("docker logs [container]", "Show container logs", "docker"),
            new("docker exec -it [container] bash", "Execute command in container", "docker"),
            new("docker pull [image]", "Pull image from registry", "docker"),
            new("docker push [image]", "Push image to registry", "docker"),
            new("docker images", "List images", "docker"),
            new("docker volume ls", "List volumes", "docker"),
            new("docker network ls", "List networks", "docker")
        },

        ["kubectl"] = new List<OfflineCheatSheetItem>
        {
            new("kubectl get pods", "List pods", "kubernetes"),
            new("kubectl get services", "List services", "kubernetes"),
            new("kubectl get deployments", "List deployments", "kubernetes"),
            new("kubectl describe pod [name]", "Describe pod", "kubernetes"),
            new("kubectl logs [pod]", "Show pod logs", "kubernetes"),
            new("kubectl exec -it [pod] -- bash", "Execute command in pod", "kubernetes"),
            new("kubectl apply -f [file]", "Apply configuration", "kubernetes"),
            new("kubectl delete pod [name]", "Delete pod", "kubernetes"),
            new("kubectl port-forward [pod] [port]:[port]", "Port forward", "kubernetes"),
            new("kubectl get nodes", "List nodes", "kubernetes"),
            new("kubectl top pods", "Show pod resource usage", "kubernetes"),
            new("kubectl rollout restart deployment [name]", "Restart deployment", "kubernetes")
        },

        ["linux"] = new List<OfflineCheatSheetItem>
        {
            new("ls -la", "List files with details", "linux"),
            new("cd [directory]", "Change directory", "linux"),
            new("pwd", "Show current directory", "linux"),
            new("mkdir [directory]", "Create directory", "linux"),
            new("rm [file]", "Remove file", "linux"),
            new("rm -rf [directory]", "Remove directory recursively", "linux"),
            new("cp [source] [dest]", "Copy file", "linux"),
            new("mv [source] [dest]", "Move/rename file", "linux"),
            new("find . -name \"[pattern]\"", "Find files by name", "linux"),
            new("grep \"[pattern]\" [file]", "Search in file", "linux"),
            new("ps aux", "List running processes", "linux"),
            new("kill [pid]", "Kill process by ID", "linux"),
            new("sudo [command]", "Run command as root", "linux"),
            new("chmod +x [file]", "Make file executable", "linux"),
            new("tar -xzf [file.tar.gz]", "Extract tar.gz file", "linux"),
            new("df -h", "Show disk usage", "linux"),
            new("free -h", "Show memory usage", "linux")
        },

        ["npm"] = new List<OfflineCheatSheetItem>
        {
            new("npm install", "Install dependencies", "npm"),
            new("npm install [package]", "Install package", "npm"),
            new("npm install -g [package]", "Install package globally", "npm"),
            new("npm run [script]", "Run npm script", "npm"),
            new("npm start", "Start application", "npm"),
            new("npm test", "Run tests", "npm"),
            new("npm build", "Build application", "npm"),
            new("npm list", "List installed packages", "npm"),
            new("npm outdated", "Check for outdated packages", "npm"),
            new("npm update", "Update packages", "npm"),
            new("npm uninstall [package]", "Uninstall package", "npm"),
            new("npm init", "Initialize new project", "npm"),
            new("npm publish", "Publish package", "npm")
        },

        ["vim"] = new List<OfflineCheatSheetItem>
        {
            new("i", "Enter insert mode", "vim"),
            new("Esc", "Exit insert mode", "vim"),
            new(":w", "Save file", "vim"),
            new(":q", "Quit vim", "vim"),
            new(":wq", "Save and quit", "vim"),
            new(":q!", "Quit without saving", "vim"),
            new("/[pattern]", "Search forward", "vim"),
            new("?[pattern]", "Search backward", "vim"),
            new("n", "Next search result", "vim"),
            new("N", "Previous search result", "vim"),
            new("dd", "Delete line", "vim"),
            new("yy", "Copy line", "vim"),
            new("p", "Paste", "vim"),
            new("u", "Undo", "vim"),
            new("Ctrl+r", "Redo", "vim"),
            new("gg", "Go to beginning", "vim"),
            new("G", "Go to end", "vim")
        },

        ["python"] = new List<OfflineCheatSheetItem>
        {
            new("python -m venv venv", "Create virtual environment", "python"),
            new("source venv/bin/activate", "Activate virtual environment (Linux/Mac)", "python"),
            new("venv\\Scripts\\activate", "Activate virtual environment (Windows)", "python"),
            new("pip install [package]", "Install package", "python"),
            new("pip install -r requirements.txt", "Install from requirements file", "python"),
            new("pip freeze > requirements.txt", "Generate requirements file", "python"),
            new("python -m pip list", "List installed packages", "python"),
            new("python -c \"import [module]; print([module].__version__)\"", "Check module version", "python"),
            new("python -m http.server 8000", "Start simple HTTP server", "python"),
            new("python -m json.tool file.json", "Pretty print JSON", "python"),
            new("python -m pdb script.py", "Debug script with pdb", "python"),
            new("python -m pytest", "Run tests with pytest", "python")
        },

        ["javascript"] = new List<OfflineCheatSheetItem>
        {
            new("node --version", "Check Node.js version", "javascript"),
            new("npm init -y", "Initialize package.json", "javascript"),
            new("npm install --save [package]", "Install and save to dependencies", "javascript"),
            new("npm install --save-dev [package]", "Install and save to devDependencies", "javascript"),
            new("npm run [script]", "Run npm script", "javascript"),
            new("npx [command]", "Execute package binary", "javascript"),
            new("console.log()", "Print to console", "javascript"),
            new("JSON.stringify(obj, null, 2)", "Pretty print JSON", "javascript"),
            new("Object.keys(obj)", "Get object keys", "javascript"),
            new("Array.from({length: n}, (_, i) => i)", "Create array of numbers", "javascript"),
            new("fetch('/api/data').then(r => r.json())", "Fetch API call", "javascript"),
            new("setTimeout(() => {}, 1000)", "Set timeout", "javascript")
        },

        ["powershell"] = new List<OfflineCheatSheetItem>
        {
            new("Get-Help [cmdlet]", "Get help for cmdlet", "powershell"),
            new("Get-Command *[keyword]*", "Find commands", "powershell"),
            new("Get-Process", "List running processes", "powershell"),
            new("Get-Service", "List services", "powershell"),
            new("Stop-Process -Name [name]", "Stop process by name", "powershell"),
            new("Start-Service [name]", "Start service", "powershell"),
            new("Get-ChildItem -Recurse", "List files recursively", "powershell"),
            new("Test-Path [path]", "Check if path exists", "powershell"),
            new("New-Item -ItemType Directory [name]", "Create directory", "powershell"),
            new("Copy-Item [source] [dest]", "Copy file/folder", "powershell"),
            new("Get-Content [file]", "Read file content", "powershell"),
            new("Set-Content [file] [content]", "Write to file", "powershell"),
            new("Invoke-WebRequest [url]", "Make HTTP request", "powershell"),
            new("ConvertTo-Json [object]", "Convert to JSON", "powershell")
        },

        ["bash"] = new List<OfflineCheatSheetItem>
        {
            new("echo $SHELL", "Show current shell", "bash"),
            new("which [command]", "Find command location", "bash"),
            new("history", "Show command history", "bash"),
            new("!!", "Repeat last command", "bash"),
            new("!n", "Repeat command number n", "bash"),
            new("alias ll='ls -la'", "Create alias", "bash"),
            new("export VAR=value", "Set environment variable", "bash"),
            new("echo $VAR", "Print environment variable", "bash"),
            new("for i in {1..10}; do echo $i; done", "For loop", "bash"),
            new("if [ -f file ]; then echo exists; fi", "If statement", "bash"),
            new("command1 && command2", "Run command2 if command1 succeeds", "bash"),
            new("command1 || command2", "Run command2 if command1 fails", "bash"),
            new("command > file.txt", "Redirect output to file", "bash"),
            new("command >> file.txt", "Append output to file", "bash"),
            new("command1 | command2", "Pipe output", "bash")
        },

        ["sql"] = new List<OfflineCheatSheetItem>
        {
            new("SELECT * FROM table", "Select all from table", "sql"),
            new("SELECT col1, col2 FROM table WHERE condition", "Select with condition", "sql"),
            new("INSERT INTO table (col1, col2) VALUES (val1, val2)", "Insert data", "sql"),
            new("UPDATE table SET col1 = val1 WHERE condition", "Update data", "sql"),
            new("DELETE FROM table WHERE condition", "Delete data", "sql"),
            new("CREATE TABLE table (id INT PRIMARY KEY, name VARCHAR(50))", "Create table", "sql"),
            new("ALTER TABLE table ADD COLUMN col_name datatype", "Add column", "sql"),
            new("DROP TABLE table", "Delete table", "sql"),
            new("SELECT COUNT(*) FROM table", "Count rows", "sql"),
            new("SELECT * FROM table ORDER BY col ASC/DESC", "Order results", "sql"),
            new("SELECT * FROM table LIMIT 10", "Limit results", "sql"),
            new("SELECT t1.*, t2.* FROM table1 t1 JOIN table2 t2 ON t1.id = t2.id", "Join tables", "sql")
        },

        ["regex"] = new List<OfflineCheatSheetItem>
        {
            new(".", "Match any character", "regex"),
            new("*", "Match 0 or more", "regex"),
            new("+", "Match 1 or more", "regex"),
            new("?", "Match 0 or 1", "regex"),
            new("^", "Start of string", "regex"),
            new("$", "End of string", "regex"),
            new("\\d", "Match digit", "regex"),
            new("\\w", "Match word character", "regex"),
            new("\\s", "Match whitespace", "regex"),
            new("[abc]", "Match any of a, b, c", "regex"),
            new("[a-z]", "Match lowercase letter", "regex"),
            new("[0-9]", "Match digit", "regex"),
            new("(group)", "Capture group", "regex"),
            new("(?:group)", "Non-capturing group", "regex"),
            new("\\1", "Back reference to group 1", "regex")
        },

        ["aws"] = new List<OfflineCheatSheetItem>
        {
            new("aws configure", "Configure AWS CLI", "aws"),
            new("aws s3 ls", "List S3 buckets", "aws"),
            new("aws s3 cp file s3://bucket/", "Upload file to S3", "aws"),
            new("aws s3 sync . s3://bucket/", "Sync directory to S3", "aws"),
            new("aws ec2 describe-instances", "List EC2 instances", "aws"),
            new("aws ec2 start-instances --instance-ids i-1234567890abcdef0", "Start EC2 instance", "aws"),
            new("aws ec2 stop-instances --instance-ids i-1234567890abcdef0", "Stop EC2 instance", "aws"),
            new("aws lambda list-functions", "List Lambda functions", "aws"),
            new("aws logs describe-log-groups", "List CloudWatch log groups", "aws"),
            new("aws iam list-users", "List IAM users", "aws"),
            new("aws cloudformation list-stacks", "List CloudFormation stacks", "aws"),
            new("aws rds describe-db-instances", "List RDS instances", "aws")
        },

        ["azure"] = new List<OfflineCheatSheetItem>
        {
            new("az login", "Login to Azure", "azure"),
            new("az account list", "List subscriptions", "azure"),
            new("az account set --subscription [id]", "Set active subscription", "azure"),
            new("az group list", "List resource groups", "azure"),
            new("az vm list", "List virtual machines", "azure"),
            new("az vm start --name [name] --resource-group [rg]", "Start VM", "azure"),
            new("az vm stop --name [name] --resource-group [rg]", "Stop VM", "azure"),
            new("az storage account list", "List storage accounts", "azure"),
            new("az webapp list", "List web apps", "azure"),
            new("az functionapp list", "List function apps", "azure"),
            new("az keyvault list", "List key vaults", "azure"),
            new("az monitor log-analytics workspace list", "List Log Analytics workspaces", "azure")
        },

        ["terraform"] = new List<OfflineCheatSheetItem>
        {
            new("terraform init", "Initialize Terraform", "terraform"),
            new("terraform plan", "Show execution plan", "terraform"),
            new("terraform apply", "Apply changes", "terraform"),
            new("terraform destroy", "Destroy infrastructure", "terraform"),
            new("terraform validate", "Validate configuration", "terraform"),
            new("terraform fmt", "Format configuration files", "terraform"),
            new("terraform show", "Show current state", "terraform"),
            new("terraform state list", "List resources in state", "terraform"),
            new("terraform state show [resource]", "Show resource details", "terraform"),
            new("terraform import [resource] [id]", "Import existing resource", "terraform"),
            new("terraform workspace list", "List workspaces", "terraform"),
            new("terraform workspace new [name]", "Create workspace", "terraform")
        },

        ["ansible"] = new List<OfflineCheatSheetItem>
        {
            new("ansible-playbook playbook.yml", "Run playbook", "ansible"),
            new("ansible-playbook playbook.yml --check", "Dry run playbook", "ansible"),
            new("ansible-playbook playbook.yml --limit host", "Run on specific host", "ansible"),
            new("ansible all -m ping", "Ping all hosts", "ansible"),
            new("ansible all -m setup", "Gather facts", "ansible"),
            new("ansible-inventory --list", "List inventory", "ansible"),
            new("ansible-vault create secret.yml", "Create encrypted file", "ansible"),
            new("ansible-vault edit secret.yml", "Edit encrypted file", "ansible"),
            new("ansible-galaxy install role", "Install role", "ansible"),
            new("ansible-config dump", "Show configuration", "ansible")
        },

        ["tmux"] = new List<OfflineCheatSheetItem>
        {
            new("tmux new -s session", "Create named session", "tmux"),
            new("tmux attach -t session", "Attach to session", "tmux"),
            new("tmux list-sessions", "List sessions", "tmux"),
            new("Ctrl+b d", "Detach from session", "tmux"),
            new("Ctrl+b c", "Create new window", "tmux"),
            new("Ctrl+b n", "Next window", "tmux"),
            new("Ctrl+b p", "Previous window", "tmux"),
            new("Ctrl+b %", "Split pane vertically", "tmux"),
            new("Ctrl+b \"", "Split pane horizontally", "tmux"),
            new("Ctrl+b arrow", "Switch pane", "tmux"),
            new("Ctrl+b x", "Close pane", "tmux"),
            new("Ctrl+b z", "Zoom pane", "tmux")
        }
    };

    public static List<CheatSheetItem> Search(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<CheatSheetItem>();

        var results = new List<CheatSheetItem>();
        var term = searchTerm.ToLowerInvariant().Trim();
        
        // Check if the search term contains a category prefix
        string categoryPrefix = null;
        string commandTerm = term;
        
        foreach (var category in OfflineSheets.Keys)
        {
            if (term.StartsWith(category + " "))
            {
                categoryPrefix = category;
                commandTerm = term.Substring(category.Length).Trim();
                break;
            }
        }

        foreach (var category in OfflineSheets)
        {
            // If a category prefix was found, only search in that category
            if (categoryPrefix != null && category.Key != categoryPrefix)
                continue;
                
            var searchInCategory = categoryPrefix != null ? commandTerm : term;
            
            var categoryResults = category.Value
                .Where(item =>
                    item.Command.ToLowerInvariant().Contains(searchInCategory) ||
                    item.Description.ToLowerInvariant().Contains(searchInCategory) ||
                    (categoryPrefix == null && category.Key.Contains(term)) ||
                    FuzzyMatcher.IsFuzzyMatch(searchInCategory, item.Command, 40))
                .Select(item => new CheatSheetItem
                {
                    Title = item.Command,
                    Description = item.Description,
                    Command = item.Command,
                    Url = $"offline://{category.Key}",
                    SourceName = $"📴 {category.Key.ToUpperInvariant()}", // Include category in source name
                    Score = CalculateOfflineScore(searchInCategory, item.Command, item.Description) + 
                           (categoryPrefix != null ? 20 : 0) // Boost score for category-specific searches
                })
                .ToList();

            results.AddRange(categoryResults);
        }

        return results
            .OrderByDescending(r => r.Score)
            .Take(10)
            .ToList();
    }

    public static List<CheatSheetItem> GetByCategory(string category)
    {
        if (!OfflineSheets.ContainsKey(category.ToLowerInvariant()))
            return new List<CheatSheetItem>();

        var categoryKey = category.ToLowerInvariant();
        return OfflineSheets[categoryKey]
            .Select(item => new CheatSheetItem
            {
                Title = item.Command,
                Description = item.Description,
                Command = item.Command,
                Url = $"offline://{categoryKey}",
                SourceName = $"📴 {categoryKey.ToUpperInvariant()}", // Make category visible in uppercase
                Score = 70
            })
            .ToList();
    }

    private static int CalculateOfflineScore(string searchTerm, string command, string description)
    {
        var score = 60; // Base score for offline content

        var commandLower = command.ToLowerInvariant();
        var descLower = description.ToLowerInvariant();

        if (commandLower.Equals(searchTerm))
            score += 40;
        else if (commandLower.StartsWith(searchTerm))
            score += 30;
        else if (commandLower.Contains(searchTerm))
            score += 20;

        if (descLower.Contains(searchTerm))
            score += 10;

        // Fuzzy matching bonus
        var fuzzyScore = FuzzyMatcher.CalculateFuzzyScore(searchTerm, command);
        score += fuzzyScore / 5; // Scale down fuzzy score

        return score;
    }
}

public record OfflineCheatSheetItem(string Command, string Description, string Category);