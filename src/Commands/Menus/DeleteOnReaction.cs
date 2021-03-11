using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;

namespace Espeon {
    public class DeleteOnReaction : MenuBase {
        private readonly Snowflake _userId;
        private readonly Func<Task<IUserMessage>> _messageFunc;

        public DeleteOnReaction(Snowflake userId, Func<Task<IUserMessage>> messageFunc) {
            this._userId = userId;
            this._messageFunc = messageFunc;
        }

        protected override async Task<IUserMessage> InitialiseAsync() {
            return await this._messageFunc();
        }

        [Button("🚮")]
        public Task Delete(ButtonEventArgs args) {
            if (args.User.Id == this._userId) {
                _ = Message.DeleteAsync();
            }
            
            return Task.CompletedTask;
        }
    }
}