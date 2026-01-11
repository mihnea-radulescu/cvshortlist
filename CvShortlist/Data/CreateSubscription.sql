declare @subscriptionTier nvarchar(25) = 'Basic Tier',
        @jobOpeningsAvailable smallint = 2,
        @maxCandidateCvsPerJobOpening smallint = 250,
        @applicationUserId nvarchar(450) = 'application_user_id'

declare @currentDate smalldatetime = sysutcdatetime()
declare @subscriptionId uniqueidentifier = newid(),
        @jobOpeningsAvailableFor3Months smallint = @jobOpeningsAvailable * 3,
        @dateCreated smalldatetime = @currentDate,
        @dateUpdated smalldatetime = @currentDate,
        @doesRenew bit = 'true'

insert into dbo.Subscriptions
(Id, SubscriptionTier, JobOpeningsAvailable, MaxCandidateCvsPerJobOpening,
 DateCreated, DateUpdated, DoesRenew, ApplicationUserId)
values (@subscriptionId, @subscriptionTier, @jobOpeningsAvailableFor3Months, @maxCandidateCvsPerJobOpening,
        @dateCreated, @dateUpdated, @doesRenew, @applicationUserId)

 declare @notificationId uniqueidentifier = newid(),
         @title nvarchar(150) = '''' + @subscriptionTier + ''' subscription added',
         @content nvarchar(max) = 'You have been subscribed to the ''' + @subscriptionTier + ''' subscription. ' +
                                  'The subscription grants you ' +
                                  cast(@jobOpeningsAvailableFor3Months as nvarchar(2)) + ' jobs with ' +
                                  cast(@maxCandidateCvsPerJobOpening as nvarchar(4)) + ' candidate CVs per job. ' +
                                  '<a href="/Subscriptions">Click here</a> to access your subscription.',
         @dateOfExpiration smalldatetime = dateadd(month, 6, @currentDate)

insert into dbo.Notifications (Id, Title, Content, DateCreated, DateOfExpiration, ApplicationUserId)
values (@notificationId, @title, @content, @dateCreated, @dateOfExpiration, @applicationUserId)
