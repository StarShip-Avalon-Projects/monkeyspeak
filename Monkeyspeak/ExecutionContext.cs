﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monkeyspeak.Logging;

namespace Monkeyspeak
{
    internal class ExecutionContext
    {
        private readonly Page page;
        private readonly TriggerBlock triggerBlock;
        private readonly TriggerReader reader;
        private readonly Dictionary<Trigger, TriggerHandler> handlers;
        private bool canContinue, prevCanContinue;
        private Trigger previous, current;

        public ExecutionContext(Page page, TriggerBlock triggerBlock, params object[] args)
        {
            this.page = page;
            this.triggerBlock = triggerBlock;
            handlers = page.handlers;

            reader = new TriggerReader(page, triggerBlock)
            {
                Parameters = args ?? new object[0]
            };
        }

        public void Run(int triggerIndex)
        {
            int j = 0;
            for (j = triggerIndex; j <= triggerBlock.Count - 1; j++)
            {
                ExecuteTrigger(triggerBlock, ref j, reader);
                // if j is -1 is used for flow triggers to break out of them, I know, a hack but it works
                if (j < 0) break;
            }
        }

        internal void ExecuteTrigger(TriggerBlock triggerBlock, ref int index, TriggerReader reader)
        {
            previous = current;
            //Logger.Debug($"{previous} Index = {index}");
            current = triggerBlock[index];
            handlers.TryGetValue(current, out TriggerHandler handler);

            if (handler == null)
            {
                throw new TriggerHanderNotFoundException($"No handler found for {current}");
            }

            reader.Trigger = current;
            reader.CurrentBlockIndex = index;

            try
            {
                if (previous.Category == TriggerCategory.Condition)
                    prevCanContinue = canContinue;
                else prevCanContinue = false;
                canContinue = handler != null ? handler(reader) : false;
                if (Logger.DebugEnabled) Logger.Debug<Page>($"{page.GetTriggerDescription(current, true)} returned {canContinue}");

                if (reader.CurrentBlockIndex == -1)
                {
                    index = reader.CurrentBlockIndex;
                    return;
                }

                if (!canContinue)
                {
                    bool found = false;
                    switch (current.Category)
                    {
                        case TriggerCategory.Cause:
                            // skip ahead for another cause to meet
                            index = triggerBlock.IndexOfTrigger(TriggerCategory.Cause, startIndex: index + 1);
                            break;

                        case TriggerCategory.Condition:
                            if (previous.Category == TriggerCategory.Condition && !prevCanContinue)
                                index = triggerBlock.IndexOfTrigger(TriggerCategory.Condition, startIndex: index + 1);
                            else if (previous.Category == TriggerCategory.Effect)
                                index = triggerBlock.IndexOfTrigger(TriggerCategory.Condition, startIndex: index + 1);
                            break;

                        case TriggerCategory.Flow:
                            // skip ahead for another flow trigger to meet
                            index = triggerBlock.IndexOfTrigger(TriggerCategory.Flow, startIndex: index + 1);
                            break;
                    }
                    if (index == -1)
                    {
                        index = triggerBlock.Count;
                    }
                }
                else
                {
                    // can continue
                    switch (current.Category)
                    {
                        case TriggerCategory.Cause:
                            Trigger possibleCause = Trigger.Undefined;
                            for (int i = index + 1; i <= triggerBlock.Count - 1; i++)
                            {
                                possibleCause = triggerBlock[i];
                                if (possibleCause.Category == TriggerCategory.Cause)
                                {
                                    index = i; // set the current index of the outer loop
                                    break;
                                }
                            }
                            break;

                        case TriggerCategory.Flow:
                            var indexOfOtherFlow = triggerBlock.IndexOfTrigger(TriggerCategory.Flow, startIndex: index + 1);
                            var subBlock = triggerBlock.GetSubBlock(index + 1, indexOfOtherFlow);
                            var subReader = new TriggerReader(page, subBlock) { Parameters = reader.Parameters };
                            int j = index;
                            for (int i = 0; i <= subBlock.Count - 1; i++)
                            {
                                ExecuteTrigger(subBlock, ref i, subReader);
                                j += i;
                                if (i == -1)
                                {
                                    break;
                                }
                            }
                            if (j == -1)
                                index = j + 1;
                            else index -= 1;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                index = triggerBlock.Count;
                page.OnError(handlers[current], current, e);
            }
        }
    }
}