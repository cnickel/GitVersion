namespace GitFlowVersion
{
    using System;
    using System.Linq;
    using LibGit2Sharp;

    public class GitVersionFinder
    {
        public VersionAndBranch FindVersion(GitVersionContext context)
        {
            EnsureMainTopologyConstraints(context);

            var hasDevelopBranch = context.Repository.FindBranch("develop") != null;

            if (hasDevelopBranch)
            {
                return new GitFlowVersionFinder().FindVersion(context);
            }
            return new GitHubFlowVersionFinder().FindVersion(context);
        }

        void EnsureMainTopologyConstraints(GitVersionContext context)
        {
            EnsureLocalBranchExists(context.Repository, "master");
            // TODO somehow enforce this? EnsureLocalBranchExists(context.Repository, "develop");
            EnsureHeadIsNotDetached(context);
        }

        void EnsureHeadIsNotDetached(GitVersionContext context)
        {
            if (!context.CurrentBranch.CanonicalName.Equals("(no branch)", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var message = string.Format("It looks like the branch being examined is a detached Head pointing to commit '{0}'. Without a proper branch name GitFlowVersion cannot determine the build version.", context.CurrentBranch.Tip.Id.ToString(7));
            throw new ErrorException(message);
        }

        void EnsureLocalBranchExists(IRepository repository, string branchName)
        {
            if (repository.FindBranch(branchName) != null)
            {
                return;
            }

            var existingBranches = string.Format("'{0}'", string.Join("', '", repository.Branches.Select(x => x.CanonicalName)));
            throw new ErrorException(string.Format("This repository doesn't contain a branch named '{0}'. Please create one. Existing branches: {1}", branchName, existingBranches));
        }
    }
}