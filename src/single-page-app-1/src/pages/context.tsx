import { getContext } from '@microsoft/power-apps/app';

export default async function ContextPage() {

    const ctx = await getContext();

    // Now you can access these context properties
    const appId = ctx.app.appId
    const environmentId = ctx.app.environmentId
    const queryParams = ctx.app.queryParams
    const fullName = ctx.user.fullName
    const objectId = ctx.user.objectId
    const tenantId = ctx.user.tenantId
    const userPrincipalName = ctx.user.userPrincipalName
    const sessionId = ctx.host.sessionId

    return (
        <section>
            <h1>Context Information</h1>
            <ul>
                <li><strong>App ID:</strong> {appId}</li>
                <li><strong>Environment ID:</strong> {environmentId}</li>
                <li><strong>Query Params:</strong> {JSON.stringify(queryParams)}</li>
                <li><strong>User Full Name:</strong> {fullName}</li>
                <li><strong>User Object ID:</strong> {objectId}</li>
                <li><strong>User Tenant ID:</strong> {tenantId}</li>
                <li><strong>User Principal Name:</strong> {userPrincipalName}</li>
                <li><strong>Session ID:</strong> {sessionId}</li>
            </ul>
        </section>
    )
}